using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using ProjetoTccBackend.Database.Requests.Competition;
using ProjetoTccBackend.Database.Requests.Exercise;
using ProjetoTccBackend.Database.Responses.Judge;
using ProjetoTccBackend.Exceptions.Judge;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Repositories.Interfaces;
using ProjetoTccBackend.Services;
using ProjetoTccBackend.Services.Interfaces;
using Xunit;
using JudgeSubmissionResponseEnum = ProjetoTccBackend.Enums.Judge.JudgeSubmissionResponse;

namespace ProjetoTCCBackend.Unit.Test.Services
{
    public class JudgeServiceTests
    {
        private Mock<IHttpClientFactory> _httpClientFactoryMock;
        private Mock<IExerciseRepository> _exerciseRepositoryMock;
        private Mock<ITokenService> _tokenServiceMock;
        private Mock<IMemoryCache> _memoryCacheMock;
        private Mock<ILogger<JudgeService>> _loggerMock;
        private Mock<HttpMessageHandler> _httpMessageHandlerMock;

        public JudgeServiceTests()
        {
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _exerciseRepositoryMock = new Mock<IExerciseRepository>();
            _tokenServiceMock = new Mock<ITokenService>();
            _memoryCacheMock = new Mock<IMemoryCache>();
            _loggerMock = new Mock<ILogger<JudgeService>>();
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        }

        private JudgeService CreateService()
        {
            var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost:8000")
            };

            _httpClientFactoryMock.Setup(f => f.CreateClient("JudgeAPI")).Returns(httpClient);

            return new JudgeService(
                _httpClientFactoryMock.Object,
                _exerciseRepositoryMock.Object,
                _tokenServiceMock.Object,
                _memoryCacheMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task AuthenticateJudge_ReturnsToken_WhenSuccessful()
        {
            // Arrange
            var expectedToken = "test-jwt-token";
            var authResponse = new JudgeAuthenticationResponse { AccessToken = expectedToken };

            _tokenServiceMock.Setup(t => t.GenerateJudgeToken()).Returns("generated-token");

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(
                    new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(
                            System.Text.Json.JsonSerializer.Serialize(authResponse)
                        )
                    }
                );

            var service = CreateService();

            // Act
            var result = await service.AuthenticateJudge();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedToken, result);
            _tokenServiceMock.Verify(t => t.GenerateJudgeToken(), Times.Once);
        }

        [Fact]
        public async Task AuthenticateJudge_ReturnsNull_WhenExceptionOccurs()
        {
            // Arrange
            _tokenServiceMock.Setup(t => t.GenerateJudgeToken()).Returns("generated-token");

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new HttpRequestException("Network error"));

            var service = CreateService();

            // Act
            var result = await service.AuthenticateJudge();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task FetchJudgeToken_ReturnsTokenFromCache_WhenValid()
        {
            // Arrange
            var cachedToken = "cached-jwt-token";
            object cacheValue = cachedToken;

            _memoryCacheMock.Setup(m => m.TryGetValue(It.IsAny<object>(), out cacheValue)).Returns(true);
            _tokenServiceMock.Setup(t => t.ValidateToken(cachedToken)).Returns(true);

            var service = CreateService();

            // Act
            var result = await service.FetchJudgeToken();

            // Assert
            Assert.Equal(cachedToken, result);
            _tokenServiceMock.Verify(t => t.ValidateToken(cachedToken), Times.Once);
        }

        [Fact]
        public async Task SendGroupExerciseAttempt_ThrowsException_WhenExerciseNotFound()
        {
            // Arrange
            var request = new GroupExerciseAttemptWorkerRequest { ExerciseId = 999, Code = "test code" };

            _exerciseRepositoryMock.Setup(r => r.GetById(999)).Returns(() => null!);

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<ExerciseNotFoundException>(
                () => service.SendGroupExerciseAttempt(request)
            );
        }

        [Fact]
        public async Task SendGroupExerciseAttempt_ReturnsValidResponse()
        {
            // Arrange
            var request = new GroupExerciseAttemptWorkerRequest { ExerciseId = 1, Code = "test code" };

            var exercise = new Exercise
            {
                Id = 1,
                JudgeUuid = "test-uuid",
                Title = "Test Exercise"
            };

            _exerciseRepositoryMock.Setup(r => r.GetById(1)).Returns(exercise);

            var service = CreateService();

            // Act
            var result = await service.SendGroupExerciseAttempt(request);

            // Assert
            Assert.IsType<JudgeSubmissionResponseEnum>(result);
            Assert.Contains(
                result,
                new[]
                {
                    JudgeSubmissionResponseEnum.Accepted,
                    JudgeSubmissionResponseEnum.WrongAnswer,
                    JudgeSubmissionResponseEnum.CompilationError,
                    JudgeSubmissionResponseEnum.TimeLimitExceeded,
                    JudgeSubmissionResponseEnum.MemoryLimitExceeded,
                    JudgeSubmissionResponseEnum.RuntimeError,
                    JudgeSubmissionResponseEnum.PresentationError,
                    JudgeSubmissionResponseEnum.SecurityError
                }
            );
        }

        [Fact]
        public async Task GetExerciseByUuidAsync_ReturnsNull_WhenAuthenticationFails()
        {
            // Arrange
            var judgeUuid = "test-uuid";

            _tokenServiceMock.Setup(t => t.GenerateJudgeToken()).Returns("token");

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new HttpRequestException("Auth failed"));

            var service = CreateService();

            // Act
            var result = await service.GetExerciseByUuidAsync(judgeUuid);

            // Assert
            Assert.Null(result);
        }
    }
}
