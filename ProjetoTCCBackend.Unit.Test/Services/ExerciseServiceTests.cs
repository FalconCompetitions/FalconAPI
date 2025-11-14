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
        private TccDbContext _dbContext;
        private Mock<ILogger<ExerciseService>> _loggerMock;

        public ExerciseServiceTests()
        {
            _exerciseRepoMock = new Mock<IExerciseRepository>();
            _inputRepoMock = new Mock<IExerciseInputRepository>();
            _outputRepoMock = new Mock<IExerciseOutputRepository>();
            _judgeServiceMock = new Mock<IJudgeService>();
            _attachedFileServiceMock = new Mock<IAttachedFileService>();
            _dbContext = DbContextTestFactory.Create();
            _loggerMock = new Mock<ILogger<ExerciseService>>();
        }

        private ExerciseService CreateService()
        {
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
            _exerciseRepoMock.Setup(r => r.GetById(1)).Returns(expectedExercise);

            // Act
            var result = await service.GetExerciseByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Test Description", result.Description);
            Assert.Equal("Test Title", result.Title);
        }

        [Fact]
        public async Task GetExerciseByIdAsync_ReturnsNull_WhenExerciseNotFound()
        {
            // Arrange
            _exerciseRepoMock.Setup(r => r.GetById(It.IsAny<int>())).Returns((Exercise)null);
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
            Assert.Equal("Exercise 1", result[0].Title);
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
                new Exercise
                {
                    Id = 4,
                    Title = "Exercise 4",
                    Description = "Desc 4",
                },
                new Exercise
                {
                    Id = 5,
                    Title = "Exercise 5",
                    Description = "Desc 5",
                },
            }
                .AsQueryable()
                .BuildMock();

            _exerciseRepoMock.Setup(r => r.Query()).Returns(exercises);
            var service = CreateService();

            // Act
            var result = await service.GetExercisesAsync(page: 1, pageSize: 2);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.TotalCount);
            Assert.Equal(3, result.TotalPages);
            Assert.Equal(2, result.Items.Count());
            Assert.Equal(1, result.Page);
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
            }
                .AsQueryable()
                .BuildMock();

            _exerciseRepoMock.Setup(r => r.Query()).Returns(exercises);
            var service = CreateService();

            // Act
            var result = await service.GetExercisesAsync(page: 1, pageSize: 10, exerciseTypeId: 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalCount);
            Assert.All(result.Items, item => Assert.Equal(1, item.ExerciseTypeId));
        }

        [Fact]
        public async Task DeleteExerciseAsync_DeletesExerciseSuccessfully()
        {
            // Arrange
            var exercise = new Exercise
            {
                Id = 1,
                Title = "Test Exercise",
                Description = "Test",
                AttachedFile = new AttachedFile
                {
                    Id = 1,
                    Name = "test.pdf",
                    FilePath = "/path/test.pdf",
                    Size = 1024,
                    Type = "application/pdf",
                },
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

            _exerciseRepoMock
                .Setup(r => r.Query())
                .Returns(new List<Exercise> { exercise }.AsQueryable());
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
            _exerciseRepoMock.Verify(r => r.Remove(It.IsAny<Exercise>()), Times.Once);
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
                Size = 1024,
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
                JudgeUuid = "test-uuid",
            };

            // Mock a file with content (Length > 0) but invalid format
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(1024); // File has content

            var request = new UpdateExerciseRequest
            {
                Title = "Updated Exercise",
                Description = "Updated Description",
                ExerciseTypeId = 1,
                Inputs = new List<UpdateExerciseInputRequest>(),
                Outputs = new List<UpdateExerciseOutputRequest>(),
            };

            _exerciseRepoMock.Setup(r => r.GetById(1)).Returns(exercise);
            _inputRepoMock
                .Setup(r =>
                    r.Find(
                        It.IsAny<System.Linq.Expressions.Expression<System.Func<
                            ExerciseInput,
                            bool
                        >>>()
                    )
                )
                .Returns(new List<ExerciseInput>().AsQueryable());
            _outputRepoMock
                .Setup(r =>
                    r.Find(
                        It.IsAny<System.Linq.Expressions.Expression<System.Func<
                            ExerciseOutput,
                            bool
                        >>>()
                    )
                )
                .Returns(new List<ExerciseOutput>().AsQueryable());
            _attachedFileServiceMock
                .Setup(s => s.IsSubmittedFileValid(fileMock.Object))
                .Returns(false); // Invalid file format
            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidAttachedFileException>(() =>
                service.UpdateExerciseAsync(1, fileMock.Object, request)
            );
        }

        [Fact]
        public async Task UpdateExerciseAsync_UpdatesExerciseWithoutFile_WhenFileIsNull()
        {
            // Arrange
            var exercise = new Exercise
            {
                Id = 1,
                Title = "Old Title",
                Description = "Old Description",
                AttachedFileId = 1,
                JudgeUuid = "test-uuid",
                ExerciseTypeId = 1,
            };

            var request = new UpdateExerciseRequest
            {
                Title = "Updated Exercise",
                Description = "Updated Description",
                ExerciseTypeId = 2,
                Inputs = new List<UpdateExerciseInputRequest>
                {
                    new UpdateExerciseInputRequest
                    {
                        Id = 1,
                        Input = "updated input",
                        ExerciseId = 1,
                        OrderId = 0,
                    },
                },
                Outputs = new List<UpdateExerciseOutputRequest>
                {
                    new UpdateExerciseOutputRequest
                    {
                        Id = 1,
                        Output = "updated output",
                        ExerciseId = 1,
                        ExerciseInputId = 1,
                        OrderId = 0,
                    },
                },
            };

            var existingInputs = new List<ExerciseInput>
            {
                new ExerciseInput
                {
                    Id = 1,
                    ExerciseId = 1,
                    Input = "old input",
                    JudgeUuid = "test-uuid",
                },
            };

            var existingOutputs = new List<ExerciseOutput>
            {
                new ExerciseOutput
                {
                    Id = 1,
                    ExerciseId = 1,
                    ExerciseInputId = 1,
                    Output = "old output",
                    JudgeUuid = "test-uuid",
                },
            };

            var updatedExercise = new Exercise
            {
                Id = 1,
                Title = "Updated Exercise",
                Description = "Updated Description",
                ExerciseTypeId = 2,
                AttachedFileId = 1,
                JudgeUuid = "test-uuid",
                ExerciseInputs = new List<ExerciseInput>
                {
                    new ExerciseInput
                    {
                        Id = 1,
                        ExerciseId = 1,
                        Input = "updated input",
                        JudgeUuid = "test-uuid",
                    },
                },
                ExerciseOutputs = new List<ExerciseOutput>
                {
                    new ExerciseOutput
                    {
                        Id = 1,
                        ExerciseId = 1,
                        ExerciseInputId = 1,
                        Output = "updated output",
                        JudgeUuid = "test-uuid",
                    },
                },
            };

            _exerciseRepoMock.Setup(r => r.GetById(1)).Returns(exercise);
            _inputRepoMock
                .Setup(r =>
                    r.Find(
                        It.IsAny<System.Linq.Expressions.Expression<System.Func<
                            ExerciseInput,
                            bool
                        >>>()
                    )
                )
                .Returns(existingInputs.AsQueryable());
            _outputRepoMock
                .Setup(r =>
                    r.Find(
                        It.IsAny<System.Linq.Expressions.Expression<System.Func<
                            ExerciseOutput,
                            bool
                        >>>()
                    )
                )
                .Returns(existingOutputs.AsQueryable());

            var queryableMock = new List<Exercise> { updatedExercise }
                .AsQueryable()
                .BuildMock();
            _exerciseRepoMock.Setup(r => r.Query()).Returns(queryableMock);

            var service = CreateService();

            // Act - Pass null for file parameter
            var result = await service.UpdateExerciseAsync(1, null, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Exercise", result.Title);
            Assert.Equal("Updated Description", result.Description);
            Assert.Equal(2, result.ExerciseTypeId);
            // Verify that file service was NOT called since file is null
            _attachedFileServiceMock.Verify(
                s => s.IsSubmittedFileValid(It.IsAny<IFormFile>()),
                Times.Never
            );
            _attachedFileServiceMock.Verify(
                s => s.DeleteAndReplaceExistentFile(It.IsAny<int>(), It.IsAny<IFormFile>()),
                Times.Never
            );
            // Verify that inputs and outputs were updated
            _inputRepoMock.Verify(r => r.Update(It.IsAny<ExerciseInput>()), Times.Once);
            _outputRepoMock.Verify(r => r.Update(It.IsAny<ExerciseOutput>()), Times.Once);
        }

        [Fact]
        public async Task UpdateExerciseAsync_UpdatesExerciseWithFile_WhenFileIsProvided()
        {
            // Arrange
            var exercise = new Exercise
            {
                Id = 1,
                Title = "Old Title",
                Description = "Old Description",
                AttachedFileId = 1,
                JudgeUuid = "test-uuid",
                ExerciseTypeId = 1,
            };

            var newAttachedFile = new AttachedFile
            {
                Id = 2,
                Name = "newfile.pdf",
                Type = "application/pdf",
                Size = 2048,
                FilePath = "/path/newfile.pdf",
            };

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(2048);

            var request = new UpdateExerciseRequest
            {
                Title = "Updated Exercise",
                Description = "Updated Description",
                ExerciseTypeId = 2,
                Inputs = new List<UpdateExerciseInputRequest>(),
                Outputs = new List<UpdateExerciseOutputRequest>(),
            };

            var updatedExercise = new Exercise
            {
                Id = 1,
                Title = "Updated Exercise",
                Description = "Updated Description",
                ExerciseTypeId = 2,
                AttachedFileId = 2,
                JudgeUuid = "test-uuid",
                ExerciseInputs = new List<ExerciseInput>(),
                ExerciseOutputs = new List<ExerciseOutput>(),
            };

            _exerciseRepoMock.Setup(r => r.GetById(1)).Returns(exercise);
            _inputRepoMock
                .Setup(r =>
                    r.Find(
                        It.IsAny<System.Linq.Expressions.Expression<System.Func<
                            ExerciseInput,
                            bool
                        >>>()
                    )
                )
                .Returns(new List<ExerciseInput>().AsQueryable());
            _outputRepoMock
                .Setup(r =>
                    r.Find(
                        It.IsAny<System.Linq.Expressions.Expression<System.Func<
                            ExerciseOutput,
                            bool
                        >>>()
                    )
                )
                .Returns(new List<ExerciseOutput>().AsQueryable());
            _attachedFileServiceMock
                .Setup(s => s.IsSubmittedFileValid(fileMock.Object))
                .Returns(true);
            _attachedFileServiceMock
                .Setup(s => s.DeleteAndReplaceExistentFile(1, fileMock.Object))
                .ReturnsAsync(newAttachedFile);

            var queryableMock = new List<Exercise> { updatedExercise }
                .AsQueryable()
                .BuildMock();
            _exerciseRepoMock.Setup(r => r.Query()).Returns(queryableMock);

            var service = CreateService();

            // Act
            var result = await service.UpdateExerciseAsync(1, fileMock.Object, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Exercise", result.Title);
            Assert.Equal(2, result.AttachedFileId);
            // Verify that file service WAS called
            _attachedFileServiceMock.Verify(
                s => s.IsSubmittedFileValid(fileMock.Object),
                Times.Once
            );
            _attachedFileServiceMock.Verify(
                s => s.DeleteAndReplaceExistentFile(1, fileMock.Object),
                Times.Once
            );
        }
    }
}
