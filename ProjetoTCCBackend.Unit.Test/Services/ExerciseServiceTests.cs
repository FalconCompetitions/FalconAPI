using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using ProjetoTccBackend.Database;
using ProjetoTccBackend.Database.Requests.Exercise;
using ProjetoTccBackend.Database.Responses.Global;
using ProjetoTccBackend.Exceptions;
using ProjetoTccBackend.Exceptions.AttachedFile;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Repositories.Interfaces;
using ProjetoTccBackend.Services;
using ProjetoTccBackend.Services.Interfaces;
using Xunit;

namespace ProjetoTCCBackend.Unit.Test.Services
{
    public class ExerciseServiceTests
    {
        private Mock<IExerciseRepository> _exerciseRepoMock;
        private Mock<IExerciseInputRepository> _inputRepoMock;
        private Mock<IExerciseOutputRepository> _outputRepoMock;
        private Mock<IJudgeService> _judgeServiceMock;
        private Mock<IAttachedFileService> _attachedFileServiceMock;
        private Mock<ILogger<ExerciseService>> _loggerMock;
        private TccDbContext? _dbContext;

        public ExerciseServiceTests()
        {
            _exerciseRepoMock = new Mock<IExerciseRepository>();
            _inputRepoMock = new Mock<IExerciseInputRepository>();
            _outputRepoMock = new Mock<IExerciseOutputRepository>();
            _judgeServiceMock = new Mock<IJudgeService>();
            _attachedFileServiceMock = new Mock<IAttachedFileService>();
            _loggerMock = new Mock<ILogger<ExerciseService>>();
        }

        private ExerciseService CreateService()
        {
            _dbContext = DbContextTestFactory.Create($"TestDb_{System.Guid.NewGuid()}");
            return new ExerciseService(
                _exerciseRepoMock.Object,
                _inputRepoMock.Object,
                _outputRepoMock.Object,
                _judgeServiceMock.Object,
                _attachedFileServiceMock.Object,
                _dbContext,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task GetExerciseByIdAsync_ReturnsExercise_WhenExerciseExists()
        {
            // Arrange
            var expectedExercise = new Exercise
            {
                Id = 1,
                Description = "Test Description",
                Title = "Test Title",
            };
            _exerciseRepoMock.Setup(r => r.GetById(1)).Returns(expectedExercise);
            var service = CreateService();

            // Act
            var result = await service.GetExerciseByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Test Title", result.Title);
            Assert.Equal("Test Description", result.Description);
        }

        [Fact]
        public async Task GetExerciseByIdAsync_ReturnsNull_WhenExerciseDoesNotExist()
        {
            // Arrange
            _exerciseRepoMock.Setup(r => r.GetById(999)).Returns((Exercise?)null);
            var service = CreateService();

            // Act
            var result = await service.GetExerciseByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetExercisesAsync_ReturnsAllExercises()
        {
            // Arrange
            var exercises = new List<Exercise>
            {
                new Exercise
                {
                    Id = 1,
                    Title = "Exercise 1",
                    Description = "Desc 1",
                },
                new Exercise
                {
                    Id = 2,
                    Title = "Exercise 2",
                    Description = "Desc 2",
                },
                new Exercise
                {
                    Id = 3,
                    Title = "Exercise 3",
                    Description = "Desc 3",
                },
            };
            _exerciseRepoMock.Setup(r => r.GetAll()).Returns(exercises.AsQueryable());
            var service = CreateService();

            // Act
            var result = await service.GetExercisesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task GetExercisesAsync_WithPagination_ReturnsPagedResult()
        {
            // Arrange
            var exercises = new List<Exercise>
            {
                new Exercise
                {
                    Id = 1,
                    Title = "Exercise 1",
                    Description = "Desc 1",
                    ExerciseTypeId = 1,
                },
                new Exercise
                {
                    Id = 2,
                    Title = "Exercise 2",
                    Description = "Desc 2",
                    ExerciseTypeId = 1,
                },
                new Exercise
                {
                    Id = 3,
                    Title = "Exercise 3",
                    Description = "Desc 3",
                    ExerciseTypeId = 1,
                },
            };

            var mock = exercises.AsQueryable().BuildMock();
            _exerciseRepoMock.Setup(r => r.Query()).Returns(mock);
            var service = CreateService();

            // Act
            var result = await service.GetExercisesAsync(page: 1, pageSize: 2);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Items.Count());
            Assert.Equal(3, result.TotalCount);
            Assert.Equal(2, result.TotalPages);
            Assert.Equal(1, result.Page);
        }

        [Fact]
        public async Task GetExercisesAsync_WithSearch_ReturnsFilteredResults()
        {
            // Arrange
            var exercises = new List<Exercise>
            {
                new Exercise
                {
                    Id = 1,
                    Title = "Python Exercise",
                    Description = "Python coding",
                    ExerciseTypeId = 1,
                },
                new Exercise
                {
                    Id = 2,
                    Title = "Java Exercise",
                    Description = "Java coding",
                    ExerciseTypeId = 1,
                },
                new Exercise
                {
                    Id = 3,
                    Title = "C# Exercise",
                    Description = "C# coding",
                    ExerciseTypeId = 1,
                },
            };

            var mock = exercises.AsQueryable().BuildMock();
            _exerciseRepoMock.Setup(r => r.Query()).Returns(mock);
            var service = CreateService();

            // Act
            var result = await service.GetExercisesAsync(page: 1, pageSize: 10, search: "Python");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Items);
            Assert.Equal("Python Exercise", result.Items.First().Title);
        }

        [Fact]
        public async Task GetExercisesAsync_WithExerciseTypeFilter_ReturnsFilteredResults()
        {
            // Arrange
            var exercises = new List<Exercise>
            {
                new Exercise
                {
                    Id = 1,
                    Title = "Exercise 1",
                    Description = "Desc 1",
                    ExerciseTypeId = 1,
                },
                new Exercise
                {
                    Id = 2,
                    Title = "Exercise 2",
                    Description = "Desc 2",
                    ExerciseTypeId = 2,
                },
                new Exercise
                {
                    Id = 3,
                    Title = "Exercise 3",
                    Description = "Desc 3",
                    ExerciseTypeId = 1,
                },
            };

            var mock = exercises.AsQueryable().BuildMock();
            _exerciseRepoMock.Setup(r => r.Query()).Returns(mock);
            var service = CreateService();

            // Act
            var result = await service.GetExercisesAsync(page: 1, pageSize: 10, exerciseTypeId: 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Items.Count());
            Assert.All(result.Items, item => Assert.Equal(1, item.ExerciseTypeId));
        }

        [Fact]
        public async Task CreateExerciseAsync_ThrowsException_WhenFileFormatIsInvalid()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            var request = new CreateExerciseRequest
            {
                Title = "Test Exercise",
                Description = "Test Description",
                ExerciseTypeId = 1,
                Inputs = new List<CreateExerciseInputRequest>(),
                Outputs = new List<CreateExerciseOutputRequest>(),
            };

            _attachedFileServiceMock
                .Setup(s => s.IsSubmittedFileValid(fileMock.Object))
                .Returns(false);
            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidAttachedFileException>(() =>
                service.CreateExerciseAsync(request, fileMock.Object)
            );
        }

        [Fact]
        public async Task DeleteExerciseAsync_ThrowsException_WhenExerciseNotFound()
        {
            // Arrange
            var queryable = new List<Exercise>().AsQueryable();
            _exerciseRepoMock.Setup(r => r.Query()).Returns(queryable);
            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<ErrorException>(() => service.DeleteExerciseAsync(999));
        }

        [Fact]
        public async Task DeleteExerciseAsync_DeletesExerciseAndRelatedData_WhenExerciseExists()
        {
            // Arrange
            var attachedFile = new AttachedFile
            {
                Id = 1,
                Name = "test.pdf",
                Type = "application/pdf",
                size = 1024,
                FilePath = "/path/test.pdf",
            };

            var exercise = new Exercise
            {
                Id = 1,
                Title = "Test Exercise",
                Description = "Test Description",
                AttachedFileId = 1,
            };

            var inputs = new List<ExerciseInput>
            {
                new ExerciseInput
                {
                    Id = 1,
                    ExerciseId = 1,
                    Input = "input1",
                },
            };

            var outputs = new List<ExerciseOutput>
            {
                new ExerciseOutput
                {
                    Id = 1,
                    ExerciseId = 1,
                    Output = "output1",
                },
            };

            var queryable = new List<Exercise> { exercise }.AsQueryable();
            _exerciseRepoMock.Setup(r => r.Query()).Returns(queryable);
            _inputRepoMock
                .Setup(r =>
                    r.Find(
                        It.IsAny<System.Linq.Expressions.Expression<System.Func<
                            ExerciseInput,
                            bool
                        >>>()
                    )
                )
                .Returns(inputs.AsQueryable());
            _outputRepoMock
                .Setup(r =>
                    r.Find(
                        It.IsAny<System.Linq.Expressions.Expression<System.Func<
                            ExerciseOutput,
                            bool
                        >>>()
                    )
                )
                .Returns(outputs.AsQueryable());

            var service = CreateService();

            // Act
            await service.DeleteExerciseAsync(1);

            // Assert
            _outputRepoMock.Verify(
                r => r.RemoveRange(It.IsAny<IEnumerable<ExerciseOutput>>()),
                Times.Once
            );
            _inputRepoMock.Verify(
                r => r.RemoveRange(It.IsAny<IEnumerable<ExerciseInput>>()),
                Times.Once
            );
            _attachedFileServiceMock.Verify(
                s => s.DeleteAttachedFile(It.IsAny<AttachedFile>()),
                Times.Once
            );
            _exerciseRepoMock.Verify(r => r.Remove(exercise), Times.Once);
        }

        [Fact]
        public async Task UpdateExerciseAsync_ThrowsException_WhenExerciseNotFound()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            var request = new UpdateExerciseRequest
            {
                Title = "Updated Exercise",
                Description = "Updated Description",
                ExerciseTypeId = 1,
                Inputs = new List<UpdateExerciseInputRequest>(),
                Outputs = new List<UpdateExerciseOutputRequest>(),
            };

            _exerciseRepoMock.Setup(r => r.GetById(999)).Returns((Exercise?)null);
            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<ErrorException>(() =>
                service.UpdateExerciseAsync(999, fileMock.Object, request)
            );
        }

        [Fact]
        public async Task UpdateExerciseAsync_ThrowsException_WhenFileFormatIsInvalid()
        {
            // Arrange
            var exercise = new Exercise
            {
                Id = 1,
                Title = "Test",
                Description = "Test",
                AttachedFileId = 1,
            };
            var fileMock = new Mock<IFormFile>();
            var request = new UpdateExerciseRequest
            {
                Title = "Updated Exercise",
                Description = "Updated Description",
                ExerciseTypeId = 1,
                Inputs = new List<UpdateExerciseInputRequest>(),
                Outputs = new List<UpdateExerciseOutputRequest>(),
            };

            _exerciseRepoMock.Setup(r => r.GetById(1)).Returns(exercise);
            _attachedFileServiceMock
                .Setup(s => s.IsSubmittedFileValid(fileMock.Object))
                .Returns(false);
            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidAttachedFileException>(() =>
                service.UpdateExerciseAsync(1, fileMock.Object, request)
            );
        }
    }
}
