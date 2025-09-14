using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using ProjetoTccBackend.Database;
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
        [Fact]
        public async Task GetExerciseByIdAsync_ReturnsExercise()
        {
            // Arrange
            var exerciseRepoMock = new Mock<IExerciseRepository>();
            var inputRepoMock = new Mock<IExerciseInputRepository>();
            var outputRepoMock = new Mock<IExerciseOutputRepository>();
            var judgeServiceMock = new Mock<IJudgeService>();
            var dbContext = DbContextTestFactory.Create();
            var loggerMock = new Mock<ILogger<ExerciseService>>();
            var expectedExercise = new Exercise
            {
                Id = 1,
                Description = "",
                Title = "",
            };
            exerciseRepoMock.Setup(r => r.GetById(1)).Returns(expectedExercise);
            var service = new ExerciseService(
                exerciseRepoMock.Object,
                inputRepoMock.Object,
                outputRepoMock.Object,
                judgeServiceMock.Object,
                dbContext,
                loggerMock.Object
            );

            // Act
            var result = await service.GetExerciseByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }
    }
}
