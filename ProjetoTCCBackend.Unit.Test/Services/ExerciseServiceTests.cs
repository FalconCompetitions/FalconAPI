using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using ProjetoTccBackend.Database;
using ProjetoTccBackend.Database.Requests.Exercise;
using ProjetoTccBackend.Exceptions.AttachedFile;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Repositories;
using ProjetoTccBackend.Services;
using ProjetoTccBackend.Services.Interfaces;
using Xunit;

namespace ProjetoTCCBackend.Unit.Test.Services
{
    /// <summary>
    /// Unit tests for the <see cref="ExerciseService"/> class.
    /// </summary>
    public class ExerciseServiceTests : IDisposable
    {
        private readonly TccDbContext _dbContext;
        private readonly Mock<IJudgeService> _judgeServiceMock;
        private readonly Mock<IAttachedFileService> _attachedFileServiceMock;
        private readonly Mock<ILogger<ExerciseService>> _loggerMock;
        private readonly ExerciseService _exerciseService;

        public ExerciseServiceTests()
        {
            _dbContext = DbContextTestFactory.Create($"TestDb_{Guid.NewGuid()}");
            
            var exerciseRepository = new ExerciseRepository(_dbContext);
            var exerciseInputRepository = new ExerciseInputRepository(_dbContext);
            var exerciseOutputRepository = new ExerciseOutputRepository(_dbContext);
            
            _judgeServiceMock = new Mock<IJudgeService>();
            _attachedFileServiceMock = new Mock<IAttachedFileService>();
            _loggerMock = new Mock<ILogger<ExerciseService>>();

            _exerciseService = new ExerciseService(
                exerciseRepository,
                exerciseInputRepository,
                exerciseOutputRepository,
                _judgeServiceMock.Object,
                _attachedFileServiceMock.Object,
                _dbContext,
                _loggerMock.Object
            );
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
        }

        [Fact]
        public async Task CreateExerciseAsync_CreatesExercise_Successfully()
        {
            // Arrange
            var exerciseType = new ExerciseType { Id = 1, Label = "Algorithm" };
            _dbContext.ExerciseTypes.Add(exerciseType);
            await _dbContext.SaveChangesAsync();

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("test.pdf");
            
            var attachedFile = new AttachedFile
            {
                Id = 1,
                Name = "test.pdf",
                Type = "application/pdf",
                Size = 1024,
                FilePath = "/uploads/test.pdf",
                CreatedAt = DateTime.UtcNow
            };

            _attachedFileServiceMock.Setup(s => s.IsSubmittedFileValid(It.IsAny<IFormFile>())).Returns(true);
            _attachedFileServiceMock.Setup(s => s.ProcessAndSaveFile(It.IsAny<IFormFile>()))
                .ReturnsAsync(attachedFile);

            var request = new CreateExerciseRequest
            {
                ExerciseTypeId = 1,
                Title = "Test Exercise",
                Description = "Test Description",
                Inputs = new List<CreateExerciseInputRequest>
                {
                    new CreateExerciseInputRequest { Input = "1 2" }
                },
                Outputs = new List<CreateExerciseOutputRequest>
                {
                    new CreateExerciseOutputRequest { Output = "3" }
                }
            };

            // Act
            var result = await _exerciseService.CreateExerciseAsync(request, fileMock.Object);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Exercise", result.Title);
            Assert.Equal("Test Description", result.Description);
            Assert.Equal(1, result.ExerciseTypeId);
        }

        [Fact]
        public async Task CreateExerciseAsync_ThrowsException_WhenFileIsInvalid()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            _attachedFileServiceMock.Setup(s => s.IsSubmittedFileValid(It.IsAny<IFormFile>())).Returns(false);

            var request = new CreateExerciseRequest
            {
                ExerciseTypeId = 1,
                Title = "Test Exercise",
                Description = "Test Description",
                Inputs = new List<CreateExerciseInputRequest>(),
                Outputs = new List<CreateExerciseOutputRequest>()
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidAttachedFileException>(
                () => _exerciseService.CreateExerciseAsync(request, fileMock.Object)
            );
        }

        [Fact]
        public async Task GetExerciseByIdAsync_ReturnsExercise_WhenExists()
        {
            // Arrange
            var exerciseType = new ExerciseType { Id = 1, Label = "Algorithm" };
            var exercise = new Exercise
            {
                Id = 1,
                Title = "Test Exercise",
                Description = "Test Description",
                ExerciseTypeId = 1,
                EstimatedTime = TimeSpan.FromMinutes(30)
            };

            _dbContext.ExerciseTypes.Add(exerciseType);
            _dbContext.Exercises.Add(exercise);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _exerciseService.GetExerciseByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Exercise", result.Title);
        }

        [Fact]
        public async Task GetExerciseByIdAsync_ReturnsNull_WhenNotExists()
        {
            // Act
            var result = await _exerciseService.GetExerciseByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteExerciseAsync_DeletesExercise_Successfully()
        {
            // Arrange
            var exerciseType = new ExerciseType { Id = 1, Label = "Algorithm" };
            var exercise = new Exercise
            {
                Id = 1,
                Title = "Test Exercise",
                Description = "Test Description",
                ExerciseTypeId = 1,
                EstimatedTime = TimeSpan.FromMinutes(30)
            };

            _dbContext.ExerciseTypes.Add(exerciseType);
            _dbContext.Exercises.Add(exercise);
            await _dbContext.SaveChangesAsync();

            // Act
            await _exerciseService.DeleteExerciseAsync(1);

            // Assert
            var deletedExercise = await _exerciseService.GetExerciseByIdAsync(1);
            Assert.Null(deletedExercise);
        }
    }
}

