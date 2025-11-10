using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using ProjetoTccBackend.Database;
using ProjetoTccBackend.Database.Requests.Exercise;
using ProjetoTccBackend.Database.Responses.Global;
using ProjetoTccBackend.Exceptions;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Repositories.Interfaces;
using ProjetoTccBackend.Services;
using ProjetoTccBackend.Services.Interfaces;
using Xunit;
using Microsoft.EntityFrameworkCore;

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
        public async Task GetExerciseByIdAsync_ReturnsExercise()
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
                new Exercise { Id = 1, Title = "Exercise 1", Description = "Desc 1" },
                new Exercise { Id = 2, Title = "Exercise 2", Description = "Desc 2" },
                new Exercise { Id = 3, Title = "Exercise 3", Description = "Desc 3" }
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
                new Exercise { Id = 1, Title = "Exercise 1", Description = "Desc 1" },
                new Exercise { Id = 2, Title = "Exercise 2", Description = "Desc 2" },
                new Exercise { Id = 3, Title = "Exercise 3", Description = "Desc 3" },
                new Exercise { Id = 4, Title = "Exercise 4", Description = "Desc 4" },
                new Exercise { Id = 5, Title = "Exercise 5", Description = "Desc 5" }
            }.AsQueryable();

            _exerciseRepoMock.Setup(r => r.Query()).Returns(exercises);
            var service = CreateService();

            // Act
            var result = await service.GetExercisesAsync(page: 1, pageSize: 2);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.TotalCount);
            Assert.Equal(3, result.TotalPages);
            Assert.Equal(2, result.Items.Count);
            Assert.Equal(1, result.Page);
        }

        [Fact]
        public async Task GetExercisesAsync_WithSearch_ReturnsFilteredResults()
        {
            // Arrange
            var exercises = new List<Exercise>
            {
                new Exercise { Id = 1, Title = "Python Exercise", Description = "Python basics" },
                new Exercise { Id = 2, Title = "Java Exercise", Description = "Java basics" },
                new Exercise { Id = 3, Title = "Python Advanced", Description = "Advanced Python" }
            }.AsQueryable();

            _exerciseRepoMock.Setup(r => r.Query()).Returns(exercises);
            var service = CreateService();

            // Act
            var result = await service.GetExercisesAsync(page: 1, pageSize: 10, search: "Python");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.TotalCount);
            Assert.All(result.Items, item => 
                Assert.True(item.Title.Contains("Python") || item.Description.Contains("Python"))
            );
        }

        [Fact]
        public async Task GetExercisesAsync_WithExerciseTypeFilter_ReturnsFilteredResults()
        {
            // Arrange
            var exercises = new List<Exercise>
            {
                new Exercise { Id = 1, Title = "Exercise 1", Description = "Desc 1", ExerciseTypeId = 1 },
                new Exercise { Id = 2, Title = "Exercise 2", Description = "Desc 2", ExerciseTypeId = 2 },
                new Exercise { Id = 3, Title = "Exercise 3", Description = "Desc 3", ExerciseTypeId = 1 }
            }.AsQueryable();

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
        public async Task DeleteExerciseAsync_ThrowsException_WhenExerciseNotFound()
        {
            // Arrange
            _exerciseRepoMock.Setup(r => r.Query()).Returns(new List<Exercise>().AsQueryable());
            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<ErrorException>(() => service.DeleteExerciseAsync(999));
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
                AttachedFile = new AttachedFile { Id = 1, FileName = "test.pdf" }
            };

            var inputs = new List<ExerciseInput>
            {
                new ExerciseInput { Id = 1, ExerciseId = 1, Input = "input1" }
            };

            var outputs = new List<ExerciseOutput>
            {
                new ExerciseOutput { Id = 1, ExerciseId = 1, Output = "output1" }
            };

            _exerciseRepoMock.Setup(r => r.Query())
                .Returns(new List<Exercise> { exercise }.AsQueryable());
            _inputRepoMock.Setup(r => r.Find(It.IsAny<System.Linq.Expressions.Expression<System.Func<ExerciseInput, bool>>>()))
                .Returns(inputs.AsQueryable());
            _outputRepoMock.Setup(r => r.Find(It.IsAny<System.Linq.Expressions.Expression<System.Func<ExerciseOutput, bool>>>()))
                .Returns(outputs.AsQueryable());

            var service = CreateService();

            // Act
            await service.DeleteExerciseAsync(1);

            // Assert
            _outputRepoMock.Verify(r => r.RemoveRange(It.IsAny<IEnumerable<ExerciseOutput>>()), Times.Once);
            _inputRepoMock.Verify(r => r.RemoveRange(It.IsAny<IEnumerable<ExerciseInput>>()), Times.Once);
            _attachedFileServiceMock.Verify(s => s.DeleteAttachedFile(It.IsAny<AttachedFile>()), Times.Once);
            _exerciseRepoMock.Verify(r => r.Remove(It.IsAny<Exercise>()), Times.Once);
        }
    }
}
