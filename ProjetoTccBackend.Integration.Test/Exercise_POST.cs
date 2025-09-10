using ProjetoTccBackend.Database.Requests.Exercise;
using System.Net;
using Xunit.Abstractions;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ProjetoTccBackend.Services.Interfaces;
using System.Net.Http.Json;

namespace ProjetoTccBackend.Integration.Test
{
    public class Exercise_POST : IClassFixture<TCCWebApplicationFactory>, IAsyncLifetime
    {
        private readonly TCCWebApplicationFactory _factory;
        private readonly ITestOutputHelper _output;

        public Exercise_POST(TCCWebApplicationFactory factory, ITestOutputHelper output)
        {
            this._factory = factory;
            this._output = output;
        }

        public async Task InitializeAsync()
        {

        }

        public async Task DisposeAsync()
        {

        }

        [Fact]
        public async Task CreateExercise_Returns_StatusCreated()
        {
            // Arrange
            string expectedUuid = Guid.NewGuid().ToString();
            var judgeServiceMock = new Mock<IJudgeService>();
            judgeServiceMock.Setup(r => r.CreateJudgeExerciseAsync(It.IsAny<CreateExerciseRequest>()))
                .Returns(Task.FromResult(expectedUuid))
                .Verifiable();

            HttpClient client = this._factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll(typeof(IJudgeService));
                    services.AddScoped(_ =>  judgeServiceMock.Object);
                });
            }).CreateClient();



            CreateExerciseRequest request = new CreateExerciseRequest()
            {
                Title = "Teste",
                Description = "Descrição",
                EstimatedTime = TimeSpan.FromMinutes(20),
                Inputs = new List<CreateExerciseInputRequest>()
                {
                    new CreateExerciseInputRequest()
                    {
                        Input = "example",
                        OrderId = 1
                    }
                },
                Outputs = new List<CreateExerciseOutputRequest>()
                {
                    new CreateExerciseOutputRequest()
                    {
                        Output = "example",
                        OrderId = 1
                    }
                }
            };
            HttpStatusCode expectedResponse = HttpStatusCode.Created;

            // Act
            HttpResponseMessage? response = await client.PostAsJsonAsync<CreateExerciseRequest>("/api/exercise", request);
            

            // Assert
            Assert.NotNull(response);
            Assert.Equal(expectedResponse, response.StatusCode);

            // Verifies that the CreateJudgeExerciseAsync method of the IJudgeService mock 
            // was called exactly once during the test execution with any instance of 
            // CreateExerciseRequest as its parameter.
            judgeServiceMock.Verify(s => s.CreateJudgeExerciseAsync(It.IsAny<CreateExerciseRequest>()), Times.Once());
        }

        [Fact]
        public async Task CreateExercise_Returns_StatusBadRequest()
        {
            // Arrange
            string expectedUuid = Guid.NewGuid().ToString();
            var judgeServiceMock = new Mock<IJudgeService>();
            judgeServiceMock.Setup(r => r.CreateJudgeExerciseAsync(It.IsAny<CreateExerciseRequest>()))
                .Returns(Task.FromResult(expectedUuid))
                .Verifiable();

            HttpClient client = this._factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll(typeof(IJudgeService));
                    services.AddScoped(_ => judgeServiceMock.Object);
                });
            }).CreateClient();



            CreateExerciseRequest request = new CreateExerciseRequest()
            {
                Title = "Teste",
                Description = "Descrição",
                EstimatedTime = TimeSpan.FromMinutes(20),
                Inputs = new List<CreateExerciseInputRequest>()
                {
                    new CreateExerciseInputRequest()
                    {
                        Input = "",
                        OrderId = 1
                    }
                },
                Outputs = new List<CreateExerciseOutputRequest>()
                {
                    new CreateExerciseOutputRequest()
                    {
                        Output = "",
                        OrderId = 1
                    }
                }
            };
            HttpStatusCode expectedResponse = HttpStatusCode.BadRequest;

            // Act
            HttpResponseMessage? response = await client.PostAsJsonAsync<CreateExerciseRequest>("/api/exercise", request);


            // Assert
            Assert.NotNull(response);
            Assert.Equal(expectedResponse, response.StatusCode);

            // Verifies that the CreateJudgeExerciseAsync method of the IJudgeService mock 
            // was called exactly once during the test execution with any instance of 
            // CreateExerciseRequest as its parameter.
            judgeServiceMock.Verify(s => s.CreateJudgeExerciseAsync(It.IsAny<CreateExerciseRequest>()), Times.Never());
        }
    }
}
