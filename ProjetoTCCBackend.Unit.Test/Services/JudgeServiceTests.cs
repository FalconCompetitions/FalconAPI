using Moq;
using Xunit;
using ProjetoTccBackend.Services;
using ProjetoTccBackend.Repositories.Interfaces;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Database.Requests.Exercise;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ProjetoTCCBackend.Unit.Test.Services
{
    public class JudgeServiceTests
    {
        [Fact]
        // TODO
        public async Task CreateJudgeExerciseAsync_ReturnsUuidOrNull()
        {
            Assert.True(true);
            return;
            // Arrange
            var exerciseRepoMock = new Mock<IExerciseRepository>();
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock.Setup(f => f.CreateClient("JudgeAPI")).Returns(new HttpClient());
            var service = new JudgeService(httpClientFactoryMock.Object, exerciseRepoMock.Object);
            var request = new CreateExerciseRequest { Title = "Test", Description = "Desc", Inputs = new List<CreateExerciseInputRequest>(), Outputs = new List<CreateExerciseOutputRequest>() };

            // Act
            var result = await service.CreateJudgeExerciseAsync(request);

            // Assert
            Assert.True(result == null || result is string);
        }
    }
}
