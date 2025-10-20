using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using ProjetoTccBackend.Database.Requests.Competition;
using ProjetoTccBackend.Database.Requests.Group;
using ProjetoTccBackend.Database.Requests.Log;
using ProjetoTccBackend.Database.Responses.Competition;
using ProjetoTccBackend.Database.Responses.Exercise;
using ProjetoTccBackend.Enums.Log;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Services.Interfaces;
using ProjetoTccBackend.Workers.Queues;

namespace ProjetoTccBackend.Hubs
{
    [Authorize]
    public class CompetitionHub : Hub
    {
        private readonly IGroupAttemptService _groupAttemptService;
        private readonly ICompetitionService _competitionService;
        private readonly ILogService _logService;
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAcessor;
        private readonly IMemoryCache _memoryCache;
        private readonly ExerciseSubmissionQueue _exerciseSubmissionQueue;
        private readonly Logger<CompetitionHub> _logger;
        private const string CompetitionCacheKey = "currentCompetition";

        public CompetitionHub(
            IGroupAttemptService groupAttemptService,
            ICompetitionService competitionService,
            IUserService userService,
            ILogService logService,
            IHttpContextAccessor httpContextAcessor,
            IMemoryCache memoryCache,
            ExerciseSubmissionQueue exerciseSubmissionQueue,
            Logger<CompetitionHub> logger
        )
        {
            this._groupAttemptService = groupAttemptService;
            this._competitionService = competitionService;
            this._userService = userService;
            this._logService = logService;
            this._httpContextAcessor = httpContextAcessor;
            this._memoryCache = memoryCache;
            this._exerciseSubmissionQueue = exerciseSubmissionQueue;
            this._logger = logger;
        }

        /// <summary>
        /// Asynchronously retrieves the current competition, either from the cache or by querying the competition
        /// service.
        /// </summary>
        /// <remarks>If the competition is retrieved from the service, it is cached with an expiration
        /// time based on the competition's end time.</remarks>
        /// <returns>The current <see cref="Competition"/> if one is available; otherwise, <see langword="null"/>.</returns>
        private async Task<Competition?> FetchCurrentCompetitionAsync()
        {
            if (_memoryCache.TryGetValue(CompetitionCacheKey, out Competition? competition))
            {
                return competition;
            }

            competition = await this._competitionService.GetCurrentCompetition();

            if (competition is not null)
            {
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(
                    competition.EndTime!.Value
                );

                this._memoryCache.Set(CompetitionCacheKey, competition, cacheEntryOptions);
            }

            return competition;
        }

        private HttpContext GetHubHttpContext()
        {
            var httpContext = this._httpContextAcessor.HttpContext;

            if (httpContext is null)
            {
                throw new Exception();
            }

            return httpContext;
        }

        private ClaimsPrincipal GetHubContextUser()
        {
            var user = Context.User;

            if (user is null)
            {
                throw new Exception();
            }

            return user;
        }

        /// <summary>
        /// Handles a new client connection to the competition hub.
        /// </summary>
        /// <remarks>
        /// Upon connection, the method retrieves the current competition and checks the user's roles. Depending on the
        /// roles, the user is added to appropriate groups (Admins, Teachers, Students). It also logs the login action and sends
        /// the competition details back to the caller. If the user has no valid roles, the connection is aborted.
        /// </remarks>
        /// <returns></returns>
        public override async Task OnConnectedAsync()
        {
            var currentCompetition = await this.FetchCurrentCompetitionAsync();

            if (currentCompetition is null)
            {
                await this.Clients.Caller.SendAsync("OnConnectionResponse", null);
                return;
            }

            HttpContext currentHttpContext = this.GetHubHttpContext();

            ClaimsPrincipal user = this.GetHubContextUser();
            var loggedUser = this._userService.GetHttpContextLoggedUser();
            bool isInvalid = false;

            if (user.IsInRole("Admin"))
            {
                await this.Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
            }
            else if (user.IsInRole("Teacher"))
            {
                await this.Groups.AddToGroupAsync(Context.ConnectionId, "Teachers");
            }
            else if (user.IsInRole("Student"))
            {
                await this.Groups.AddToGroupAsync(Context.ConnectionId, "Students");
                await this.Groups.AddToGroupAsync(Context.ConnectionId, loggedUser.Id);
            }
            else
            {
                isInvalid = true;
                this._logger.LogCritical("Usuário não possui nenhuma role válida");
            }

            if (isInvalid == false)
            {
                await this._logService.CreateLogAsync(
                    new CreateLogRequest()
                    {
                        UserId = loggedUser.Id,
                        ActionType = LogType.Login,
                        CompetitionId = currentCompetition!.Id,
                        GroupId = loggedUser.GroupId,
                        IpAddress = currentHttpContext.Connection.RemoteIpAddress!.ToString(),
                    }
                );
            }

            if (isInvalid == true)
            {
                this.Context.Abort();
                return;
            }

            await this.Clients.Caller.SendAsync(
                "OnConnectionResponse",
                new Competition()
                {
                    Id = currentCompetition.Id,
                    Name = currentCompetition.Name,
                    Description = currentCompetition.Description,
                    StartTime = currentCompetition.StartTime,
                    EndTime = currentCompetition.EndTime,
                    StartInscriptions = currentCompetition.StartInscriptions,
                    EndInscriptions = currentCompetition.EndInscriptions,
                    BlockSubmissions = currentCompetition.BlockSubmissions,
                    Status = currentCompetition.Status,
                    MaxExercises = currentCompetition.MaxExercises,
                    MaxMembers = currentCompetition.MaxMembers,
                    MaxSubmissionSize = currentCompetition.MaxSubmissionSize,
                    Duration = currentCompetition.Duration,
                    StopRanking = currentCompetition.StopRanking,
                    SubmissionPenalty = currentCompetition.SubmissionPenalty,
                    Exercices = currentCompetition.Exercices,
                    Groups = currentCompetition.Groups,
                    CompetitionRankings = currentCompetition.CompetitionRankings,
                }
            );

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var currentCompetition = await this.FetchCurrentCompetitionAsync();

            ClaimsPrincipal user = this.GetHubContextUser();
            User loggedUser = this._userService.GetHttpContextLoggedUser();
            HttpContext httpContext = this.GetHubHttpContext();

            await this._logService.CreateLogAsync(
                new CreateLogRequest()
                {
                    UserId = loggedUser.Id,
                    CompetitionId = currentCompetition.Id,
                    GroupId = loggedUser.GroupId,
                    ActionType = LogType.Logout,
                    IpAddress = httpContext.Connection.RemoteIpAddress!.ToString(),
                }
            );

            await base.OnDisconnectedAsync(exception);
        }

        public async Task Ping()
        {
            await Clients.Caller.SendAsync("Pong", new { message = "Pong" });
        }

        /// <summary>
        /// Submits an exercise attempt for processing in the current competition.
        /// </summary>
        /// <remarks>This method enqueues the exercise attempt for asynchronous processing. If there is no
        /// active competition,  a null response is sent back to the caller.</remarks>
        /// <param name="request">The exercise attempt details, including the group and exercise data.</param>
        /// <returns></returns>
        [Authorize(Roles = "Student")]
        public async Task SendExerciseAttempt(GroupExerciseAttemptRequest request)
        {
            Competition? currentCompetition = await this.FetchCurrentCompetitionAsync();

            if (currentCompetition is null)
            {
                await Clients.Caller.SendAsync("ReceiveExerciseAttemptResponse", null);
                return;
            }

            var queueItem = new ExerciseSubmissionQueueItem(request, this.Context.ConnectionId);

            await _exerciseSubmissionQueue.EnqueueAsync(queueItem);
        }

        [Authorize(Roles = "Student")]
        public async Task SendCompetitionQuestion(CreateGroupQuestionRequest request)
        {
            var competition = await this.FetchCurrentCompetitionAsync();

            if (competition is null)
            {
                await this.Clients.Caller.SendAsync("ReceiveQuestionCreationResponse", null);
                return;
            }

            User loggedUser = this._userService.GetHttpContextLoggedUser();

            if (loggedUser.GroupId is null)
            {
                throw new Exception("Usuário não pertence a nenhum grupo");
            }

            Question question = await this._competitionService.CreateGroupQuestion(
                loggedUser,
                request
            );

            await Clients.Caller.SendAsync("ReceiveQuestionCreationResponse", question);
            await Clients.Group("Teachers").SendAsync("ReceiveQuestionCreation", question);
            await Clients.Group("Admins").SendAsync("ReceiveQuestionCreation", question);
        }

        [Authorize(Roles = "Admin,Teacher")]
        public async Task AnswerQuestion(AnswerGroupQuestionRequest request)
        {
            var competition = await this.FetchCurrentCompetitionAsync();

            if (competition is null)
            {
                await this.Clients.Caller.SendAsync("ReceiveQuestionAnswerResponse", null);
                return;
            }

            User loggedUser = this._userService.GetHttpContextLoggedUser();

            Answer answer = await this._competitionService.AnswerGroupQuestion(loggedUser, request);

            await Clients.Caller.SendAsync("ReceiveQuestionAnswerResponse", answer);
            await Clients.Group("Teachers").SendAsync("ReceiveQuestionAnswer", answer);
            await Clients.Group("Admins").SendAsync("ReceiveQuestionAnswer", answer);
        }

        [Authorize(Roles = "Admin,Teacher")]
        public async Task ChangeJudgeSubmissionResponse(RevokeGroupSubmissionRequest request)
        {
            bool succeeded = await this._groupAttemptService.ChangeGroupExerciseAttempt(
                request.SubmissionId,
                request.NewJudgeResponse
            );

            await this.Clients.Caller.SendAsync("ReceiveChangeJudgeSubmissionResponse", succeeded);
        }

        [Authorize(Roles = "Admin,Teacher")]
        public async Task BlockGroupSubmission() { }
    }
}
