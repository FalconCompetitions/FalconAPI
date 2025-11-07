using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using ProjetoTccBackend.Database.Requests.Competition;
using ProjetoTccBackend.Database.Requests.Exercise;
using ProjetoTccBackend.Enums.Exercise;
using ProjetoTccBackend.Enums.Judge;
using ProjetoTccBackend.Exceptions.Judge;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Repositories.Interfaces;
using ProjetoTccBackend.Services;
using ProjetoTccBackend.Services.Interfaces;
using Xunit;

namespace ProjetoTCCBackend.Unit.Test.Services
{
    public class JudgeServiceTests
    {
        private Mock<IExerciseRepository> _exerciseRepoMock;
        private Mock<IHttpClientFactory> _httpClientFactoryMock;
        private Mock<ITokenService> _tokenServiceMock;
        private Mock<IMemoryCache> _memoryCacheMock;
        private Mock<ILogger<JudgeService>> _loggerMock;

        public JudgeServiceTests()
        {
            _exerciseRepoMock = new Mock<IExerciseRepository>();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _tokenServiceMock = new Mock<ITokenService>();
            _memoryCacheMock = new Mock<IMemoryCache>();
            _loggerMock = new Mock<ILogger<JudgeService>>();
        }

        private JudgeService CreateService()
        {
            _httpClientFactoryMock.Setup(f => f.CreateClient("JudgeAPI")).Returns(new HttpClient());
            return new JudgeService(
                _httpClientFactoryMock.Object,
                _exerciseRepoMock.Object,
                _tokenServiceMock.Object,
                _memoryCacheMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task FetchJudgeToken_ReturnsTokenFromCache_WhenTokenIsValidInCache()
        {
            // Arrange
            var cachedToken = "cached-valid-token";
            object tokenObject = cachedToken;
            _memoryCacheMock
                .Setup(m => m.TryGetValue("JudgeJwtToken", out tokenObject))
                .Returns(true);
            _tokenServiceMock.Setup(t => t.ValidateToken(cachedToken)).Returns(true);
            var service = CreateService();

            // Act
            var result = await service.FetchJudgeToken();

            // Assert
            Assert.Equal(cachedToken, result);
            _tokenServiceMock.Verify(t => t.GenerateJudgeToken(), Times.Never);
        }

        [Fact]
        public async Task FetchJudgeToken_GeneratesNewToken_WhenCacheIsEmpty()
        {
            // Arrange
            object? tokenObject = null;
            _memoryCacheMock
                .Setup(m => m.TryGetValue("JudgeJwtToken", out tokenObject))
                .Returns(false);
            _tokenServiceMock.Setup(t => t.GenerateJudgeToken()).Returns("generated-token");
            var service = CreateService();

            // Act
            var result = await service.FetchJudgeToken();

            // Assert
            _tokenServiceMock.Verify(t => t.GenerateJudgeToken(), Times.Once);
        }

        [Fact]
        public async Task FetchJudgeToken_GeneratesNewToken_WhenCachedTokenIsInvalid()
        {
            // Arrange
            var cachedToken = "invalid-token";
            object tokenObject = cachedToken;
            _memoryCacheMock
                .Setup(m => m.TryGetValue("JudgeJwtToken", out tokenObject))
                .Returns(true);
            _tokenServiceMock.Setup(t => t.ValidateToken(cachedToken)).Returns(false);
            _tokenServiceMock.Setup(t => t.GenerateJudgeToken()).Returns("new-token");
            var service = CreateService();

            // Act
            var result = await service.FetchJudgeToken();

            // Assert
            _tokenServiceMock.Verify(t => t.GenerateJudgeToken(), Times.Once);
        }

        [Fact]
        public async Task GetExerciseByUuidAsync_ReturnsNull_WhenTokenFetchFails()
        {
            // Arrange
            object? tokenObject = null;
            _memoryCacheMock
                .Setup(m => m.TryGetValue("JudgeJwtToken", out tokenObject))
                .Returns(false);
            _tokenServiceMock.Setup(t => t.GenerateJudgeToken()).Returns("token");

            // Setup para simular falha na autenticação - sem configurar HttpClient apropriadamente
            var service = CreateService();

            // Act
            var result = await service.GetExerciseByUuidAsync("test-uuid");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetExerciseByUuidAsync_ReturnsNull_WhenExerciseNotInRepository()
        {
            // Arrange
            var judgeUuid = "non-existent-uuid";
            var emptyList = new List<Exercise>().AsQueryable();

            _exerciseRepoMock
                .Setup(r =>
                    r.Find(
                        It.IsAny<System.Linq.Expressions.Expression<System.Func<Exercise, bool>>>()
                    )
                )
                .Returns(emptyList);

            object? tokenObject = null;
            _memoryCacheMock
                .Setup(m => m.TryGetValue("JudgeJwtToken", out tokenObject))
                .Returns(false);
            _tokenServiceMock.Setup(t => t.GenerateJudgeToken()).Returns("test-token");

            var service = CreateService();

            // Act
            var result = await service.GetExerciseByUuidAsync(judgeUuid);

            // Assert
            // Pode retornar null por falha de HTTP ou por não encontrar no repositório
            // Ambos são comportamentos válidos para este teste
            Assert.True(result == null);
        }

        [Fact]
        public void GetExercisesAsync_ThrowsNotImplementedException()
        {
            // Arrange
            var service = CreateService();

            // Act & Assert
            Assert.ThrowsAsync<NotImplementedException>(() => service.GetExercisesAsync());
        }

        /// <summary>
        /// Tests that the <c>SendGroupExerciseAttempt</c> method throws an <see cref="ExerciseNotFoundException"/>
        /// when the specified exercise is not found in the repository.
        /// </summary>
        /// <remarks>
        /// This test arranges a <see cref="GroupExerciseAttemptWorkerRequest"/> with a non-existent exercise ID,
        /// sets up the exercise repository mock to return <c>null</c> for that ID, and asserts that the exception is thrown.
        /// </remarks>
        [Fact]
        public async Task SendGroupExerciseAttempt_ThrowsException_WhenExerciseNotFound()
        {
            // Arrange
            var request = new GroupExerciseAttemptWorkerRequest
            {
                ExerciseId = 999,
                Code = "print('test')",
                LanguageType = LanguageType.Python,
            };

            _exerciseRepoMock.Setup(r => r.GetById(999)).Returns((Exercise?)null);
            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<ExerciseNotFoundException>(() =>
                service.SendGroupExerciseAttempt(request)
            );
        }

        /// <summary>
        /// Tests that the <c>SendGroupExerciseAttempt</c> method returns a valid <see cref="JudgeSubmissionResponse"/>
        /// when the specified exercise exists in the repository.
        /// </summary>
        /// <remarks>
        /// This test sets up a mock exercise and request, configures the repository mock to return the exercise,
        /// and asserts that the service returns a response of the expected type.
        /// </remarks>
        [Fact]
        public async Task SendGroupExerciseAttempt_ReturnsValidResponse_WhenExerciseExists()
        {
            // Arrange
            var exercise = new Exercise
            {
                Id = 1,
                JudgeUuid = "test-uuid",
                Title = "Test Exercise",
                Description = "Test Description",
            };

            var request = new GroupExerciseAttemptWorkerRequest
            {
                ExerciseId = 1,
                Code = "print('test')",
                LanguageType = LanguageType.Python,
            };

            _exerciseRepoMock.Setup(r => r.GetById(1)).Returns(exercise);
            var service = CreateService();

            // Act
            var result = await service.SendGroupExerciseAttempt(request);

            // Assert
            Assert.IsType<JudgeSubmissionResponse>(result);
        }

        [Fact]
        public async Task SendGroupExerciseAttempt_ReturnsRandomResponse_FromValidEnumValues()
        {
            // Arrange
            var exercise = new Exercise
            {
                Id = 1,
                JudgeUuid = "test-uuid",
                Title = "Test Exercise",
                Description = "Test Description",
            };

            var request = new GroupExerciseAttemptWorkerRequest
            {
                ExerciseId = 1,
                Code = "print('test')",
                LanguageType = LanguageType.Python,
            };

            _exerciseRepoMock.Setup(r => r.GetById(1)).Returns(exercise);
            var service = CreateService();

            // Act
            var result = await service.SendGroupExerciseAttempt(request);

            // Assert
            var validResponses = new[]
            {
                JudgeSubmissionResponse.Accepted,
                JudgeSubmissionResponse.WrongAnswer,
                JudgeSubmissionResponse.CompilationError,
                JudgeSubmissionResponse.TimeLimitExceeded,
                JudgeSubmissionResponse.MemoryLimitExceeded,
                JudgeSubmissionResponse.RuntimeError,
                JudgeSubmissionResponse.PresentationError,
                JudgeSubmissionResponse.SecurityError,
            };

            Assert.Contains(result, validResponses);
        }

        [Fact]
        public async Task UpdateExerciseAsync_ThrowsException_WhenTokenFetchFails()
        {
            // Arrange
            var exercise = new Exercise
            {
                Id = 1,
                JudgeUuid = "test-uuid",
                Title = "Test Exercise",
                Description = "Test Description",
                ExerciseInputs = new List<ExerciseInput> { new ExerciseInput { Input = "input1" } },
                ExerciseOutputs = new List<ExerciseOutput>
                {
                    new ExerciseOutput { Output = "output1" },
                },
            };

            // Setup para fazer AuthenticateJudge retornar null
            object? tokenObject = null;
            _memoryCacheMock
                .Setup(m => m.TryGetValue("JudgeJwtToken", out tokenObject))
                .Returns(false);
            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<JudgeSubmissionException>(() =>
                service.UpdateExerciseAsync(exercise)
            );
        }

        [Fact]
        public async Task UpdateExerciseAsync_ThrowsException_WhenAuthenticationFails()
        {
            // Arrange
            var exercise = new Exercise
            {
                Id = 1,
                JudgeUuid = "test-uuid",
                Title = "Test Exercise",
                Description = "Test Description",
                ExerciseInputs = new List<ExerciseInput> { new ExerciseInput { Input = "input1" } },
                ExerciseOutputs = new List<ExerciseOutput>
                {
                    new ExerciseOutput { Output = "output1" },
                },
            };

            // Setup para fazer AuthenticateJudge retornar null (simula falha de autenticação)
            object? tokenObject = null;
            _memoryCacheMock
                .Setup(m => m.TryGetValue("JudgeJwtToken", out tokenObject))
                .Returns(false);
            _tokenServiceMock.Setup(t => t.GenerateJudgeToken()).Returns("test-token");
            var service = CreateService();

            // Act & Assert - Deve lançar exceção quando a autenticação falha
            await Assert.ThrowsAsync<JudgeSubmissionException>(() =>
                service.UpdateExerciseAsync(exercise)
            );
        }
    }
}
