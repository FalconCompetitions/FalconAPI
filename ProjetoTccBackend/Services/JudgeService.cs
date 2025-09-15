using System.Net;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using ProjetoTccBackend.Database.Requests.Competition;
using ProjetoTccBackend.Database.Requests.Exercise;
using ProjetoTccBackend.Database.Requests.Judge;
using ProjetoTccBackend.Database.Responses.Judge;
using ProjetoTccBackend.Exceptions.Judge;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Repositories.Interfaces;
using ProjetoTccBackend.Services.Interfaces;
using JudgeSubmissionResponseEnum = ProjetoTccBackend.Enums.Judge.JudgeSubmissionResponse;

namespace ProjetoTccBackend.Services
{
    public class JudgeService : IJudgeService
    {
        private readonly HttpClient _httpClient;
        private readonly IExerciseRepository _exerciseRepository;
        private readonly ITokenService _tokenService;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<JudgeService> _logger;
        private const string judgeMemoryToken = "JudgeJwtToken";

        public JudgeService(
            IHttpClientFactory httpClientFactory,
            IExerciseRepository exerciseRepository,
            ITokenService tokenService,
            IMemoryCache memoryCache,
            ILogger<JudgeService> logger
        )
        {
            this._httpClient = httpClientFactory.CreateClient("JudgeAPI");
            this._exerciseRepository = exerciseRepository;
            this._tokenService = tokenService;
            this._memoryCache = memoryCache;
            this._logger = logger;
        }

        /// <inheritdoc />
        public async Task<string?> AuthenticateJudge()
        {
            string generatedToken = this._tokenService.GenerateJudgeToken();

            try
            {
                HttpResponseMessage response = await this._httpClient.PostAsJsonAsync(
                    "/auth/integrator-token",
                    new { api_key = generatedToken }
                );

                JudgeAuthenticationResponse authenticationResponse =
                    await response.Content.ReadFromJsonAsync<JudgeAuthenticationResponse>();

                return authenticationResponse!.AccessToken;
            }
            catch (Exception ex)
            {
                this._logger.LogCritical("Was not possible to recover judge authentication token");
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<string?> FetchJudgeToken()
        {
            if (this._memoryCache.TryGetValue(judgeMemoryToken, out string jwtToken))
            {
                bool isTokenValid = this._tokenService.ValidateToken(jwtToken);

                if (isTokenValid)
                {
                    return jwtToken;
                }
            }

            string? newToken = await this.AuthenticateJudge();

            if (newToken == null)
            {
                return null;
            }

            this._memoryCache.Set(judgeMemoryToken, newToken);

            return newToken;
        }

        /// <inheritdoc/>
        public async Task<string?> CreateJudgeExerciseAsync(CreateExerciseRequest exerciseRequest)
        {
            string? currentToken = await this.FetchJudgeToken();

            if (currentToken is null)
            {
                return null;
            }

            this._httpClient.DefaultRequestHeaders.Add("Authorization", currentToken);

            var exerciseInputs = exerciseRequest.Inputs.ToList();
            var exerciseOutputs = exerciseRequest.Outputs.ToList();

            List<string> inputs = new List<string>();
            List<string> outputs = new List<string>();

            for (int i = 0; i < exerciseRequest.Inputs.Count; i++)
            {
                inputs.Add(Convert.ToBase64String(Encoding.UTF8.GetBytes(exerciseInputs[i].Input)));
                outputs.Add(
                    Convert.ToBase64String(Encoding.UTF8.GetBytes(exerciseOutputs[i].Output))
                );
            }

            try
            {
                CreateJudgeExerciseRequest payload = new CreateJudgeExerciseRequest()
                {
                    Name = exerciseRequest.Title,
                    Description = exerciseRequest.Description,
                    DataEntry = inputs,
                    DataOutput = outputs,
                    EntryDescription = "",
                    OutputDescription = "",
                };

                var result = await this._httpClient.PostAsJsonAsync<CreateJudgeExerciseRequest>(
                    "/problems",
                    payload
                );

                if (result.StatusCode == HttpStatusCode.Created)
                {
                    JudgeExerciseResponse? exerciseResponse =
                        await result.Content.ReadFromJsonAsync<JudgeExerciseResponse>();

                    if (exerciseResponse != null)
                    {
                        return exerciseResponse.Id;
                    }
                }
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex.ToString(), ex.StackTrace);
            }

            return null;
        }

        /// <inheritdoc/>
        public async Task<Exercise?> GetExerciseByUuidAsync(string judgeUuid)
        {
            string? currentToken = await this.FetchJudgeToken();

            if (currentToken is null)
            {
                return null;
            }

            this._httpClient.DefaultRequestHeaders.Add("Authorization", currentToken);

            var judgeExercise = await this._httpClient.GetFromJsonAsync<JudgeExerciseResponse>(
                $"/problems/{judgeUuid}"
            );

            if (judgeExercise is null)
            {
                return null;
            }

            var exercise = this
                ._exerciseRepository.Find((x) => x.JudgeUuid!.Equals(judgeUuid))
                .FirstOrDefault();

            return exercise;
        }

        /// <inheritdoc/>
        public Task<ICollection<Exercise>> GetExercisesAsync()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public async Task<JudgeSubmissionResponseEnum> SendGroupExerciseAttempt(
            GroupExerciseAttemptRequest request
        )
        {
            string? currentToken = await this.FetchJudgeToken();

            if (currentToken is null)
            {
                throw new JudgeSubmissionException("Erro em recurso externo");
            }

            this._httpClient.DefaultRequestHeaders.Add("Authorization", currentToken);

            Exercise? exercise = this._exerciseRepository.GetById(request.ExerciseId);

            if (exercise is null)
            {
                throw new ExerciseNotFoundException();
            }

            JudgeSubmissionRequest judgeRequest = new JudgeSubmissionRequest()
            {
                ProblemId = exercise.JudgeUuid!,
                Content = request.Code,
                LanguageType = request.LanguageType.ToString(),
            };

            HttpResponseMessage response =
                await this._httpClient.PostAsJsonAsync<JudgeSubmissionRequest>(
                    "/submissions",
                    judgeRequest
                );

            if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
            {
                throw new JudgeSubmissionException(
                    "Ocorreu um erro ao executar ou processar o código enviado"
                );
            }

            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new JudgeSubmissionException("Erro em recurso externo");
            }

            //STATUS_ACCEPTED = "ACCEPTED" - Aceito
            //STATUS_PRESENTATION_ERROR = "PRESENTATION ERROR" - Erro de apresentação
            //STATUS_WRONG_ANSWER = "WRONG ANSWER" - Recusado
            //STATUS_COMPILATION_ERROR = "COMPILATION ERROR" - Erro de compilação
            //STATUS_TIME_LIMIT_EXCEEDED = "TIME LIMIT EXCEEDED" - Tempo excedido
            //STATUS_MEMORY_LIMIT_EXCEEDED = "MEMORY LIMIT EXCEEDED" - Limite de memória excedido
            //STATUS_RUNTIME_ERROR = "RUNTIME ERROR" - Erro em tempo de execução
            //STATUS_SECURITY_ERROR = "SECURITY ERROR" - Erro de segurança(código  'perigoso')

            JudgeSubmissionResponse? judgeSubmissionResponse =
                await response.Content.ReadFromJsonAsync<JudgeSubmissionResponse>();

            if (judgeSubmissionResponse is null)
            {
                return JudgeSubmissionResponseEnum.RuntimeError;
            }

            switch (judgeSubmissionResponse.Status)
            {
                case "ACCEPTED":
                    return JudgeSubmissionResponseEnum.Accepted;
                case "PRESENTATION ERROR":
                    return JudgeSubmissionResponseEnum.PresentationError;
                case "WRONG ANSWER":
                    return JudgeSubmissionResponseEnum.WrongAnswer;
                case "COMPILATION ERROR":
                    return JudgeSubmissionResponseEnum.CompilationError;
                case "TIME LIMIT EXCEEDED":
                    return JudgeSubmissionResponseEnum.TimeLimitExceeded;
                case "MEMORY LIMIT EXCEEDED":
                    return JudgeSubmissionResponseEnum.MemoryLimitExceeded;
                case "RUNTIME ERROR":
                    return JudgeSubmissionResponseEnum.RuntimeError;
                case "SECURITY ERROR":
                    return JudgeSubmissionResponseEnum.SecurityError;
                default:
                    return JudgeSubmissionResponseEnum.WrongAnswer;
            }
        }

        /// <inheritdoc />
        public async Task<bool> UpdateExerciseAsync(Exercise exercise)
        {
            string? currentToken = await this.FetchJudgeToken();

            if (currentToken is null)
            {
                throw new JudgeSubmissionException("Erro em recurso externo");
            }

            this._httpClient.DefaultRequestHeaders.Add("Authorization", currentToken);

            UpdateJudgeExerciseRequest request = new UpdateJudgeExerciseRequest()
            {
                ProblemId = exercise.JudgeUuid!,
                Description = exercise.Description,
                Name = exercise.Title,
                DataEntry = exercise.ExerciseInputs.Select(x => x.Input).ToArray(),
                DataOutput = exercise.ExerciseOutputs.Select(x => x.Output).ToArray(),
            };

            HttpResponseMessage response =
                await this._httpClient.PutAsJsonAsync<UpdateJudgeExerciseRequest>(
                    $"/problems/{exercise.JudgeUuid!}",
                    request
                );

            return response.StatusCode.Equals(HttpStatusCode.OK);
        }
    }
}
