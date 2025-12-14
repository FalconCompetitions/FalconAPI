using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using ProjetoTccBackend.Database;
using ProjetoTccBackend.Exceptions;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Repositories;
using ProjetoTccBackend.Services;
using ProjetoTccBackend.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Http;
using System.Threading;

namespace ProjetoTCCBackend.Unit.Test.Services
{
    /// <summary>
    /// Unit tests for the <see cref="AttachedFileService"/> class.
    /// </summary>
    public class AttachedFileServiceTests : IDisposable
    {
        private readonly TccDbContext _dbContext;
        private readonly AttachedFileService _attachedFileService;
        private readonly AttachedFileRepository _attachedFileRepository;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<ILogger<AttachedFileService>> _mockLogger;
        private readonly IConfiguration _configuration;
        private readonly string _testFilesPath;

        public AttachedFileServiceTests()
        {
            // Create unique test database
            _dbContext = DbContextTestFactory.Create($"TestDb_{Guid.NewGuid()}");

            // Setup test files directory
            _testFilesPath = Path.Combine(Path.GetTempPath(), $"TestFiles_{Guid.NewGuid()}");
            Directory.CreateDirectory(_testFilesPath);

            // Setup real configuration with test path
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddInMemoryCollection(new Dictionary<string, string>
            {
                {"FileStorage:Path", _testFilesPath}
            });
            _configuration = configBuilder.Build();

            // Setup mock user service
            _mockUserService = new Mock<IUserService>();
            var testUser = new User
            {
                Id = "test-user-id",
                UserName = "testuser",
                Email = "test@example.com"
            };
            _mockUserService.Setup(s => s.GetHttpContextLoggedUser()).Returns(testUser);

            // Setup mock logger
            _mockLogger = new Mock<ILogger<AttachedFileService>>();

            // Create real repository
            _attachedFileRepository = new AttachedFileRepository(_dbContext);

            // Create service with real configuration
            _attachedFileService = new AttachedFileService(
                _mockUserService.Object,
                _attachedFileRepository,
                _dbContext,
                _mockLogger.Object,
                _configuration
            );
        }

        public void Dispose()
        {
            // Clean up test files
            if (Directory.Exists(_testFilesPath))
            {
                try
                {
                    Directory.Delete(_testFilesPath, recursive: true);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }

            _dbContext.Dispose();
        }

        #region IsSubmittedFileValid Tests

        /// <summary>
        /// Tests that IsSubmittedFileValid returns true for PDF files.
        /// </summary>
        [Fact]
        public void IsSubmittedFileValid_ReturnsTrue_ForPdfFile()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("document.pdf");

            // Act
            var result = _attachedFileService.IsSubmittedFileValid(mockFile.Object);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that IsSubmittedFileValid returns true for PDF files with uppercase extension.
        /// </summary>
        [Fact]
        public void IsSubmittedFileValid_ReturnsTrue_ForPdfFileUppercase()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("document.PDF");

            // Act
            var result = _attachedFileService.IsSubmittedFileValid(mockFile.Object);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that IsSubmittedFileValid returns false for non-PDF files.
        /// </summary>
        [Fact]
        public void IsSubmittedFileValid_ReturnsFalse_ForNonPdfFile()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("document.txt");

            // Act
            var result = _attachedFileService.IsSubmittedFileValid(mockFile.Object);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsSubmittedFileValid returns false for files without extension.
        /// </summary>
        [Fact]
        public void IsSubmittedFileValid_ReturnsFalse_ForFileWithoutExtension()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("document");

            // Act
            var result = _attachedFileService.IsSubmittedFileValid(mockFile.Object);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region ProcessAndSaveFile Tests

        /// <summary>
        /// Tests that ProcessAndSaveFile saves file to disk and database.
        /// </summary>
        [Fact]
        public async Task ProcessAndSaveFile_SavesFile_WhenFileIsValid()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            var fileName = "test-document.pdf";
            var content = "Test file content";
            var memoryStream = new MemoryStream();
            var writer = new StreamWriter(memoryStream);
            writer.Write(content);
            writer.Flush();
            memoryStream.Position = 0;

            mockFile.Setup(f => f.FileName).Returns(fileName);
            mockFile.Setup(f => f.ContentType).Returns("application/pdf");
            mockFile.Setup(f => f.Length).Returns(memoryStream.Length);
            mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns(async (Stream stream, CancellationToken token) =>
                {
                    memoryStream.Position = 0;
                    await memoryStream.CopyToAsync(stream, token);
                });

            // Act
            var result = await _attachedFileService.ProcessAndSaveFile(mockFile.Object);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("test-document", result.Name);
            Assert.Equal("application/pdf", result.Type);
            Assert.True(File.Exists(result.FilePath));
            Assert.True(result.FilePath.EndsWith(".pdf"));

            // Verify file in database
            var savedFile = await _dbContext.AttachedFiles.FirstOrDefaultAsync(f => f.Id == result.Id);
            Assert.NotNull(savedFile);
            Assert.Equal(result.Name, savedFile.Name);
        }

        /// <summary>
        /// Tests that ProcessAndSaveFile creates directory if it doesn't exist.
        /// </summary>
        [Fact]
        public async Task ProcessAndSaveFile_CreatesDirectory_WhenNotExists()
        {
            // Arrange
            var nonExistentPath = Path.Combine(Path.GetTempPath(), $"NonExistent_{Guid.NewGuid()}");
            
            // Create new configuration with non-existent path
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddInMemoryCollection(new Dictionary<string, string>
            {
                {"FileStorage:Path", nonExistentPath}
            });
            var newConfiguration = configBuilder.Build();

            var newService = new AttachedFileService(
                _mockUserService.Object,
                _attachedFileRepository,
                _dbContext,
                _mockLogger.Object,
                newConfiguration
            );

            var mockFile = new Mock<IFormFile>();
            var fileName = "test.pdf";
            var memoryStream = new MemoryStream(new byte[] { 1, 2, 3 });

            mockFile.Setup(f => f.FileName).Returns(fileName);
            mockFile.Setup(f => f.ContentType).Returns("application/pdf");
            mockFile.Setup(f => f.Length).Returns(memoryStream.Length);
            mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns(async (Stream stream, CancellationToken token) =>
                {
                    memoryStream.Position = 0;
                    await memoryStream.CopyToAsync(stream, token);
                });

            try
            {
                // Act
                var result = await newService.ProcessAndSaveFile(mockFile.Object);

                // Assert
                Assert.True(Directory.Exists(nonExistentPath));
                Assert.True(File.Exists(result.FilePath));
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(nonExistentPath))
                {
                    Directory.Delete(nonExistentPath, recursive: true);
                }
            }
        }

        /// <summary>
        /// Tests that ProcessAndSaveFile generates unique file names for duplicate uploads.
        /// </summary>
        [Fact]
        public async Task ProcessAndSaveFile_GeneratesUniqueFileName_ForDuplicateNames()
        {
            // Arrange
            var mockFile1 = CreateMockPdfFile("document.pdf", "Content 1");
            var mockFile2 = CreateMockPdfFile("document.pdf", "Content 2");

            // Act
            var result1 = await _attachedFileService.ProcessAndSaveFile(mockFile1.Object);
            var result2 = await _attachedFileService.ProcessAndSaveFile(mockFile2.Object);

            // Assert
            Assert.NotEqual(result1.FilePath, result2.FilePath);
            Assert.True(File.Exists(result1.FilePath));
            Assert.True(File.Exists(result2.FilePath));
            Assert.Equal("document", result1.Name);
            Assert.Equal("document", result2.Name);
        }

        #endregion

        #region DeleteAttachedFile Tests

        /// <summary>
        /// Tests that DeleteAttachedFile removes file from disk and database.
        /// </summary>
        [Fact]
        public async Task DeleteAttachedFile_RemovesFile_WhenFileExists()
        {
            // Arrange
            var mockFile = CreateMockPdfFile("delete-test.pdf", "Test content");
            var savedFile = await _attachedFileService.ProcessAndSaveFile(mockFile.Object);
            var filePath = savedFile.FilePath;

            // Act
            _attachedFileService.DeleteAttachedFile(savedFile);
            await _dbContext.SaveChangesAsync();

            // Assert
            Assert.False(File.Exists(filePath));
            var deletedFile = await _dbContext.AttachedFiles.FirstOrDefaultAsync(f => f.Id == savedFile.Id);
            Assert.Null(deletedFile);
        }

        /// <summary>
        /// Tests that DeleteAttachedFile handles missing physical files gracefully.
        /// </summary>
        [Fact]
        public async Task DeleteAttachedFile_HandlesGracefully_WhenPhysicalFileDoesNotExist()
        {
            // Arrange
            var attachedFile = new AttachedFile
            {
                FilePath = Path.Combine(_testFilesPath, "nonexistent.pdf"),
                Name = "nonexistent",
                Size = 100,
                Type = "application/pdf"
            };
            _dbContext.AttachedFiles.Add(attachedFile);
            await _dbContext.SaveChangesAsync();

            // Act & Assert - Should not throw
            _attachedFileService.DeleteAttachedFile(attachedFile);
            await _dbContext.SaveChangesAsync();

            // Verify removed from database
            var deletedFile = await _dbContext.AttachedFiles.FirstOrDefaultAsync(f => f.Id == attachedFile.Id);
            Assert.Null(deletedFile);
        }

        #endregion

        #region DeleteAndReplaceExistentFile Tests

        /// <summary>
        /// Tests that DeleteAndReplaceExistentFile deletes old file and saves new one.
        /// </summary>
        [Fact]
        public async Task DeleteAndReplaceExistentFile_ReplacesFile_WhenOldFileExists()
        {
            // Arrange
            var oldFile = CreateMockPdfFile("old-file.pdf", "Old content");
            var savedOldFile = await _attachedFileService.ProcessAndSaveFile(oldFile.Object);
            var oldFilePath = savedOldFile.FilePath;
            var oldFileId = savedOldFile.Id;

            var newFile = CreateMockPdfFile("new-file.pdf", "New content");

            // Act
            var result = await _attachedFileService.DeleteAndReplaceExistentFile(oldFileId, newFile.Object);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("new-file", result.Name);
            Assert.False(File.Exists(oldFilePath));
            Assert.True(File.Exists(result.FilePath));

            // Verify old file removed from database
            var deletedFile = await _dbContext.AttachedFiles.FirstOrDefaultAsync(f => f.Id == oldFileId);
            Assert.Null(deletedFile);

            // Verify new file in database
            var newSavedFile = await _dbContext.AttachedFiles.FirstOrDefaultAsync(f => f.Id == result.Id);
            Assert.NotNull(newSavedFile);
        }

        /// <summary>
        /// Tests that DeleteAndReplaceExistentFile saves new file when old file doesn't exist.
        /// </summary>
        [Fact]
        public async Task DeleteAndReplaceExistentFile_SavesNewFile_WhenOldFileDoesNotExist()
        {
            // Arrange
            var nonExistentId = 99999;
            var newFile = CreateMockPdfFile("new-file.pdf", "New content");

            // Act
            var result = await _attachedFileService.DeleteAndReplaceExistentFile(nonExistentId, newFile.Object);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("new-file", result.Name);
            Assert.True(File.Exists(result.FilePath));
        }

        #endregion

        #region GetFileAsync Tests

        /// <summary>
        /// Tests that GetFileAsync returns file information when file exists.
        /// </summary>
        [Fact]
        public async Task GetFileAsync_ReturnsFileInfo_WhenFileExists()
        {
            // Arrange
            var mockFile = CreateMockPdfFile("get-test.pdf", "Test content");
            var savedFile = await _attachedFileService.ProcessAndSaveFile(mockFile.Object);

            // Act
            var result = await _attachedFileService.GetFileAsync(savedFile.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(savedFile.FilePath, result.Item1);
            Assert.Equal("get-test", result.Item2);
            Assert.Equal("application/pdf", result.Item3);
        }

        /// <summary>
        /// Tests that GetFileAsync returns null when file doesn't exist in database.
        /// </summary>
        [Fact]
        public async Task GetFileAsync_ReturnsNull_WhenFileNotInDatabase()
        {
            // Arrange
            var nonExistentId = 99999;

            // Act
            var result = await _attachedFileService.GetFileAsync(nonExistentId);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetFileAsync handles files with non-absolute paths.
        /// </summary>
        [Fact]
        public async Task GetFileAsync_ReconstructsPath_WhenStoredPathIsRelative()
        {
            // Arrange
            var mockFile = CreateMockPdfFile("relative-path-test.pdf", "Test content");
            var savedFile = await _attachedFileService.ProcessAndSaveFile(mockFile.Object);
            
            // Modify the file path to be relative (simulate old data)
            var fileName = Path.GetFileName(savedFile.FilePath);
            savedFile.FilePath = fileName; // Store only filename, not full path
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _attachedFileService.GetFileAsync(savedFile.Id);

            // Assert
            Assert.NotNull(result);
            Assert.True(Path.IsPathFullyQualified(result.Item1));
            Assert.Equal("relative-path-test", result.Item2);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Creates a mock PDF file for testing.
        /// </summary>
        private Mock<IFormFile> CreateMockPdfFile(string fileName, string content)
        {
            var mockFile = new Mock<IFormFile>();
            var memoryStream = new MemoryStream();
            var writer = new StreamWriter(memoryStream);
            writer.Write(content);
            writer.Flush();
            memoryStream.Position = 0;

            mockFile.Setup(f => f.FileName).Returns(fileName);
            mockFile.Setup(f => f.ContentType).Returns("application/pdf");
            mockFile.Setup(f => f.Length).Returns(memoryStream.Length);
            mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns(async (Stream stream, CancellationToken token) =>
                {
                    memoryStream.Position = 0;
                    await memoryStream.CopyToAsync(stream, token);
                });

            return mockFile;
        }

        #endregion
    }
}
