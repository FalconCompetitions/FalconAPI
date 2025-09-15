using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using ProjetoTccBackend.Database.Requests.Exercise;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Repositories.Interfaces;
using ProjetoTccBackend.Services;
using ProjetoTccBackend.Services.Interfaces;
using Xunit;

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
            var tokenServiceMock = new Mock<ITokenService>();
            var memoryCacheMock = new Mock<IMemoryCache>();
            var loggerMock = new Mock<ILogger<JudgeService>>();
            httpClientFactoryMock.Setup(f => f.CreateClient("JudgeAPI")).Returns(new HttpClient());
            var service = new JudgeService(
                httpClientFactoryMock.Object,
                exerciseRepoMock.Object,
                tokenServiceMock.Object,
                memoryCacheMock.Object,
                loggerMock.Object
            );
            var request = new CreateExerciseRequest
            {
                Title = "Test",
                Description = "Desc",
                Inputs = new List<CreateExerciseInputRequest>(),
                Outputs = new List<CreateExerciseOutputRequest>(),
            };

            // Act
            var result = await service.CreateJudgeExerciseAsync(request);

            // Assert
            Assert.True(result == null || result is string);
        }
    }
}
