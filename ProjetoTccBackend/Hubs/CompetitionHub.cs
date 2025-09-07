using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using ProjetoTccBackend.Database.Requests.Competition;
using ProjetoTccBackend.Database.Requests.Group;
using ProjetoTccBackend.Database.Requests.Log;
using ProjetoTccBackend.Database.Responses.Exercise;
using ProjetoTccBackend.Enums.Log;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Services.Interfaces;

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
        private readonly Logger<CompetitionHub> _logger;
        private const string CompetitionCacheKey = "currentCompetition";

        public CompetitionHub(
            IGroupAttemptService groupAttemptService,
            ICompetitionService competitionService,
            IUserService userService,
            ILogService logService,
            IHttpContextAccessor httpContextAcessor,
            IMemoryCache memoryCache,
            Logger<CompetitionHub> logger
        )
        {
            this._groupAttemptService = groupAttemptService;
            this._competitionService = competitionService;
            this._userService = userService;
            this._logService = logService;
            this._httpContextAcessor = httpContextAcessor;
            this._memoryCache = memoryCache;
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
                    competition.EndTime
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

            if(isInvalid == true)
            {
                this.Context.Abort();
                return;
            }

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

        [Authorize(Roles = "Student")]
        public async Task SendExerciseAttempt(GroupExerciseAttemptRequest request)
        {
            Competition? currentCompetition = await this.FetchCurrentCompetitionAsync();

            if (currentCompetition is null)
            {
                await Clients.Caller.SendAsync("ReceiveExerciseAttemptResponse", null);
                return;
            }

            ExerciseSubmissionResponse exerciseAttempt =
                await this._groupAttemptService.SubmitExerciseAttempt(currentCompetition, request);

            await Clients.Caller.SendAsync("ReceiveExerciseAttemptResponse", exerciseAttempt);
            await Clients.Group("Teachers").SendAsync("ReceiveExerciseAttempt", exerciseAttempt);
            await Clients.Group("Admins").SendAsync("ReceiveExerciseAttempt", exerciseAttempt);
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

            Answer answer = await this._competitionService.AnswerGroupQuestion(
                loggedUser,
                request
            );

            await Clients.Caller.SendAsync("ReceiveQuestionAnswerResponse", answer);
            await Clients.Group("Teachers").SendAsync("ReceiveQuestionAnswer", answer);
            await Clients.Group("Admins").SendAsync("ReceiveQuestionAnswer", answer);
        }

        [Authorize(Roles = "Admin,Teacher")]
        public async Task RevokeJudgeSubmissionResponse(RevokeGroupSubmissionRequest request)
        {
            User loggedUser = this._userService.GetHttpContextLoggedUser();
        }
    }
}
