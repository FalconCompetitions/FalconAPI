using Microsoft.AspNetCore.SignalR;
using ProjetoTccBackend.Services.Interfaces;
using ProjetoTccBackend.Database.Requests.Group;
using ProjetoTccBackend.Database.Requests.Competition;
using ProjetoTccBackend.Models;
using Microsoft.AspNetCore.Authorization;
using ProjetoTccBackend.Database.Responses.Competition;

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
        }

        [Authorize(Roles = "Student")]
        public async Task SendCompetitionQuestion(CreateGroupQuestionRequest request)
        {
            User loggedUser = this._userService.GetHttpContextLoggerUser();

            if(loggedUser.GroupId is null)
            {
                throw new Exception("Usuário não pertence a nenhum grupo");
            }

            Question question = await this._competitionService.CreateGroupQuestion(loggedUser.Id, (int)loggedUser.GroupId!, request);

            await Clients.Caller.SendAsync("ReceiveQuestionCreationResponse", question);
        }

        
    }
}
