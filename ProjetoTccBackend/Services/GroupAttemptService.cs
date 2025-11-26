using ProjetoTccBackend.Database;
using ProjetoTccBackend.Database.Requests.Competition;
using ProjetoTccBackend.Database.Responses.Competition;
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
        private readonly IGroupRepository _groupRepository;
        private readonly ICompetitionRankingService _competitionRankingService;
        private readonly IGroupExerciseAttemptRepository _groupExerciseAttemptRepository;

        public GroupAttemptService(TccDbContext dbContext, IJudgeService judgeService, IUserService userService, IGroupRepository groupRepository, ICompetitionRankingService competitionRankingService, IGroupExerciseAttemptRepository groupExerciseAttemptRepository)
        {
            this._dbContext = dbContext;
            this._judgeService = judgeService;
            this._userService = userService;
            this._groupRepository = groupRepository;
            this._competitionRankingService = competitionRankingService;
            this._groupExerciseAttemptRepository = groupExerciseAttemptRepository;
        }


        /// <inheritdoc />
        public async Task<(ExerciseSubmissionResponse submission, CompetitionRankingResponse ranking)> SubmitExerciseAttempt(Competition currentCompetition, GroupExerciseAttemptWorkerRequest request)
        {
            try
            {
                var response = await this._judgeService.SendGroupExerciseAttempt(request);

                var lastGroupAttempt = this._groupExerciseAttemptRepository
                    .GetLastGroupCompetitionAttempt(
                        request.GroupId,
                        currentCompetition.Id
                    );


                DateTime time = (lastGroupAttempt is null)
                    ? currentCompetition.StartTime
                    : lastGroupAttempt.SubmissionTime;

                TimeSpan duration = DateTime.UtcNow.Subtract(time);


                GroupExerciseAttempt attempt = new GroupExerciseAttempt()
                {
                    Accepted = response.Equals(JudgeSubmissionResponse.Accepted),
                    Code = request.Code,
                    ExerciseId = request.ExerciseId,
                    CompetitionId = currentCompetition.Id,
                    GroupId = request.GroupId,
                    JudgeResponse = response,
                    Language = request.LanguageType,
                    SubmissionTime = DateTime.UtcNow,
                    Time = duration,
                };

                this._groupExerciseAttemptRepository.Add(attempt);
                
                // Save the attempt BEFORE calculating ranking so it's included in the count
                await this._dbContext.SaveChangesAsync();

                Group? group = this._groupRepository.GetByIdWithUsers(request.GroupId);

                if (group is null)
                {
                    throw new JudgeException($"Group with ID {request.GroupId} not found");
                }

                var rankingResponse = await this._competitionRankingService.UpdateRanking(currentCompetition, group, attempt);

                // Build the full submission response with all fields
                var exerciseResponse = new ExerciseSubmissionResponse()
                {
                    Id = attempt.Id,
                    ExerciseId = request.ExerciseId,
                    Accepted = attempt.Accepted,
                    JudgeResponse = response,
                    GroupId = request.GroupId,
                    Code = request.Code,
                    LanguageId = request.LanguageType,
                    SubmittedAt = attempt.SubmissionTime,
                    ExecutionTime = 0, // Judge doesn't return this yet
                    MemoryUsed = 0, // Judge doesn't return this yet
                    Score = attempt.Accepted ? 1 : 0,
                    Points = (int)rankingResponse.Points,
                    Penalty = (int)rankingResponse.Penalty,
                };

                return (exerciseResponse, rankingResponse);
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

            if (groupAttempt is null)
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
