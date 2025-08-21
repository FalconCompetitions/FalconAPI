using Microsoft.AspNetCore.SignalR;
using ProjetoTccBackend.Services.Interfaces;
using ProjetoTccBackend.Database.Requests.Group;
using ProjetoTccBackend.Database.Requests.Competition;
using ProjetoTccBackend.Models;
using Microsoft.AspNetCore.Authorization;
using ProjetoTccBackend.Database.Responses.Exercise;

namespace ProjetoTccBackend.Hubs
{

    [Authorize]
    public class CompetitionHub : Hub
    {
        private readonly IGroupAttemptService _groupAttemptService;
        private readonly ICompetitionService _competitionService;
        private readonly IUserService _userService;
        private readonly Logger<CompetitionHub> _logger;

        public CompetitionHub(IGroupAttemptService groupAttemptService, ICompetitionService competitionService, IUserService userService, Logger<CompetitionHub> logger)
        {
            this._groupAttemptService = groupAttemptService;
            this._competitionService = competitionService;
            this._userService = userService;
            this._logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var user = Context.User;

            if(user.IsInRole("Admin"))
            {
                await this.Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
            } else if(user.IsInRole("Teacher"))
            {
                await this.Groups.AddToGroupAsync(Context.ConnectionId, "Teachers");
            } else if(user.IsInRole("Student"))
            {
                await this.Groups.AddToGroupAsync(Context.ConnectionId, "Students");
            } else
            {
                this._logger.LogCritical("Usuário não possui nenhuma role válida");
            }

            await base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            return base.OnDisconnectedAsync(exception);
        }

        public async Task Ping()
        {
            await Clients.Caller.SendAsync("Pong", new { message = "Pong" });
        }

        [Authorize(Roles = "Student")]
        public async Task SendExerciseAttempt(GroupExerciseAttemptRequest request)
        {
            Competition? currentCompetition = await this._competitionService.GetCurrentCompetition();

            if(currentCompetition is null)
            {
                throw new Exception("Nenhuma competição em andamento");
            }

            ExerciseSubmissionResponse exerciseAttempt = await this._groupAttemptService.SubmitExerciseAttempt(currentCompetition, request);

            await Clients.Caller.SendAsync("ReceiveExerciseAttemptResponse", exerciseAttempt);
            await Clients.Group("Teachers").SendAsync("ReceiveExerciseAttempt", exerciseAttempt);
            await Clients.Group("Admins").SendAsync("ReceiveExerciseAttempt", exerciseAttempt);
            
        }

        [Authorize(Roles = "Student")]
        public async Task SendCompetitionQuestion(CreateGroupQuestionRequest request)
        {
            User loggedUser = this._userService.GetHttpContextLoggerUser();

            if(loggedUser.GroupId is null)
            {
                throw new Exception("Usuário não pertence a nenhum grupo");
            }

            Question question = await this._competitionService.CreateGroupQuestion(loggedUser, request);

            await Clients.Caller.SendAsync("ReceiveQuestionCreationResponse", question);
            await Clients.Group("Teachers").SendAsync("ReceiveQuestionCreation", question);
            await Clients.Group("Admins").SendAsync("ReceiveQuestionCreation", question);
        }

        [Authorize(Roles = "Admin,Teacher")]
        public async Task AnswerQuestion(AnswerGroupQuestionRequest request)
        {
            User loggedUser = this._userService.GetHttpContextLoggerUser();

            Question answer = await this._competitionService.AnswerGroupQuestion(loggedUser, request);

            await Clients.Caller.SendAsync("ReceiveQuestionAnswerResponse", answer);
            await Clients.Group("Teachers").SendAsync("ReceiveQuestionAnswer", answer);
            await Clients.Group("Admins").SendAsync("ReceiveQuestionAnswer", answer);
        }

        public async Task RevokeJudgeSubmissionResponse(RevokeGroupSubmissionRequest request)
        {
            User loggedUser = this._userService.GetHttpContextLoggerUser();


        }

        
    }
}
