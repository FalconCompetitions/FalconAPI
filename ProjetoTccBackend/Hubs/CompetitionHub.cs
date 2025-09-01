using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
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
        private readonly Logger<CompetitionHub> _logger;

        public CompetitionHub(
            IGroupAttemptService groupAttemptService,
            ICompetitionService competitionService,
            IUserService userService,
            ILogService logService,
            IHttpContextAccessor httpContextAcessor,
            Logger<CompetitionHub> logger
        )
        {
            this._groupAttemptService = groupAttemptService;
            this._competitionService = competitionService;
            this._userService = userService;
            this._logService = logService;
            this._httpContextAcessor = httpContextAcessor;
            this._logger = logger;
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
            ClaimsPrincipal user = this.GetHubContextUser();

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
                var currentCompetition = await this._competitionService.GetCurrentCompetition();
                var loggedUser = this._userService.GetHttpContextLoggedUser();

                await this.Groups.AddToGroupAsync(Context.ConnectionId, "Students");
                await this.Groups.AddToGroupAsync(Context.ConnectionId, loggedUser.Id);

                HttpContext currentHttpContext = this.GetHubHttpContext();

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
            else
            {
                this._logger.LogCritical("Usuário não possui nenhuma role válida");
            }

            await base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            ClaimsPrincipal user = this.GetHubContextUser();
            User loggedUser = this._userService.GetHttpContextLoggedUser();
            HttpContext httpContext = this.GetHubHttpContext();

            var currentCompetition = this._competitionService.GetCurrentCompetition();

            this._logService.CreateLogAsync(
                new CreateLogRequest()
                {
                    UserId = loggedUser.Id,
                    CompetitionId = currentCompetition.Id,
                    GroupId = loggedUser.GroupId,
                    ActionType = LogType.Logout,
                    IpAddress = httpContext.Connection.RemoteIpAddress!.ToString(),
                }
            );

            return base.OnDisconnectedAsync(exception);
        }

        public async Task Ping()
        {
            await Clients.Caller.SendAsync("Pong", new { message = "Pong" });
        }

        [Authorize(Roles = "Student")]
        public async Task SendExerciseAttempt(GroupExerciseAttemptRequest request)
        {
            Competition? currentCompetition =
                await this._competitionService.GetCurrentCompetition();

            if (currentCompetition is null)
            {
                throw new Exception("Nenhuma competição em andamento");
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
            User loggedUser = this._userService.GetHttpContextLoggedUser();

            Question answer = await this._competitionService.AnswerGroupQuestion(
                loggedUser,
                request
            );

            await Clients.Caller.SendAsync("ReceiveQuestionAnswerResponse", answer);
            await Clients.Group("Teachers").SendAsync("ReceiveQuestionAnswer", answer);
            await Clients.Group("Admins").SendAsync("ReceiveQuestionAnswer", answer);
        }

        public async Task RevokeJudgeSubmissionResponse(RevokeGroupSubmissionRequest request)
        {
            User loggedUser = this._userService.GetHttpContextLoggedUser();
        }
    }
}
