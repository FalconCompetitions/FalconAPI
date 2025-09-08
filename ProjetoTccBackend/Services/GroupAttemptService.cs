using ProjetoTccBackend.Database;
using ProjetoTccBackend.Database.Requests.Competition;
using ProjetoTccBackend.Database.Responses.Exercise;
using ProjetoTccBackend.Enums.Judge;
using ProjetoTccBackend.Exceptions.Judge;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Repositories.Interfaces;
using ProjetoTccBackend.Services.Interfaces;

namespace ProjetoTccBackend.Services
{
    public class GroupAttemptService : IGroupAttemptService
    {
        private readonly TccDbContext _dbContext;
        private readonly IJudgeService _judgeService;
        private readonly IUserService _userService;
        private readonly ICompetitionRankingService _competitionRankingService;
        private readonly IGroupExerciseAttemptRepository _groupExerciseAttemptRepository;

        public GroupAttemptService(TccDbContext dbContext, IJudgeService judgeService, IUserService userService, ICompetitionRankingService competitionRankingService, IGroupExerciseAttemptRepository groupExerciseAttemptRepository)
        {
            this._dbContext = dbContext;
            this._judgeService = judgeService;
            this._userService = userService;
            this._competitionRankingService = competitionRankingService;
            this._groupExerciseAttemptRepository = groupExerciseAttemptRepository;
        }

        
        /// <inheritdoc />
        public async Task<ExerciseSubmissionResponse> SubmitExerciseAttempt(Competition currentCompetition, GroupExerciseAttemptRequest request)
        {
            var loggedUser = this._userService.GetHttpContextLoggedUser();

            if (loggedUser is null || loggedUser.GroupId is null)
            {
                throw new UnauthorizedAccessException("Usuário não possui permissão para essa ação");
            }

            try
            {
                var response = await this._judgeService.SendGroupExerciseAttempt(request);

                var exerciseResponse = new ExerciseSubmissionResponse()
                {
                    ExerciseId = request.ExerciseId,
                    Accepted = response.Equals(JudgeSubmissionResponse.Accepted),
                    JudgeResponse = response,
                    GroupId = loggedUser.GroupId.Value,
                };

                var lastGroupAttempt = this._groupExerciseAttemptRepository
                    .GetLastGroupCompetitionAttempt(
                        (int)loggedUser.GroupId!,
                        currentCompetition.Id
                    );


                DateTime time = (lastGroupAttempt is null)
                    ? currentCompetition.StartTime
                    : lastGroupAttempt.SubmissionTime;

                TimeSpan duration = DateTime.Now.Subtract(time);


                GroupExerciseAttempt attempt = new GroupExerciseAttempt()
                {
                    Accepted = exerciseResponse.Accepted,
                    Code = request.Code,
                    ExerciseId = request.ExerciseId,
                    CompetitionId = currentCompetition.Id,
                    GroupId = (int)loggedUser.GroupId!,
                    JudgeResponse = response,
                    Language = request.LanguageType,
                    SubmissionTime = DateTime.Now,
                    Time = duration,
                };

                this._groupExerciseAttemptRepository.Add(attempt);

                await this._competitionRankingService.UpdateRanking(currentCompetition, loggedUser.Group, attempt);

                exerciseResponse.Id = attempt.Id;

                return exerciseResponse;
            }
            catch (Exception ex)
            {
                throw new JudgeException(ex.Message, ex.Data);
            }
        }


        /// <inheritdoc />
        public async Task<bool> ChangeGroupExerciseAttempt(int attemptId, JudgeSubmissionResponse newResponse)
        {
            var groupAttempt = this._groupExerciseAttemptRepository.GetById(attemptId);

            if(groupAttempt is null)
            {
                return false;
            }
            groupAttempt.JudgeResponse = newResponse;
            groupAttempt.Accepted = newResponse == JudgeSubmissionResponse.Accepted;

            this._groupExerciseAttemptRepository.Update(groupAttempt);

            await this._dbContext.SaveChangesAsync();

            return true;
        }


    }
}
