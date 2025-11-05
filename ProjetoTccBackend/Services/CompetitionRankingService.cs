using ProjetoTccBackend.Database;
using ProjetoTccBackend.Database.Responses.Competition;
using ProjetoTccBackend.Database.Responses.Group;
using ProjetoTccBackend.Database.Responses.User;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Repositories.Interfaces;
using ProjetoTccBackend.Services.Interfaces;

namespace ProjetoTccBackend.Services
{
    public class CompetitionRankingService : ICompetitionRankingService
    {
        private readonly ICompetitionRankingRepository _competitionRankingRepository;
        private readonly IGroupExerciseAttemptRepository _groupExerciseAttemptRepository;
        private readonly TccDbContext _dbContext;

        public CompetitionRankingService(ICompetitionRankingRepository competitionRankingRepository, IGroupExerciseAttemptRepository groupExerciseAttemptRepository, TccDbContext dbContext)
        {
            this._competitionRankingRepository = competitionRankingRepository;
            this._groupExerciseAttemptRepository = groupExerciseAttemptRepository;
            this._dbContext = dbContext;
        }

        /// <inheritdoc />
        public async Task<CompetitionRankingResponse> UpdateRanking(Competition competition, Models.Group group, Models.GroupExerciseAttempt exerciseAttempt)
        {
            List<Models.GroupExerciseAttempt> attempts = this._groupExerciseAttemptRepository.Find(
                e => 
                    e.GroupId.Equals(group.Id)
                    && e.CompetitionId.Equals(competition.Id)
                ).ToList();

            List<CompetitionRanking> rankings = this._competitionRankingRepository
                .Find(c =>
                    c.CompetitionId.Equals(competition.Id)
                ).ToList();

            int totalPenaltys = attempts.Count;
            double totalPenalty = totalPenaltys * competition.SubmissionPenalty.TotalMinutes;
            int totalPoints = attempts.Count(x => x.Accepted);

            int groupRankingIndex = rankings.FindIndex(x => x.GroupId.Equals(group.Id));

            CompetitionRanking updatedRanking;

            if (groupRankingIndex == -1)
            {
                CompetitionRanking newRanking = new CompetitionRanking()
                {
                    CompetitionId = competition.Id,
                    GroupId = group.Id,
                    Points = totalPoints,
                    Penalty = totalPenalty,
                };

                this._competitionRankingRepository.Add(newRanking);

                rankings.Add(newRanking);
                updatedRanking = newRanking;
            }
            else
            {
                CompetitionRanking ranking = rankings[groupRankingIndex];

                ranking.Points = totalPoints;
                ranking.Penalty = totalPenalty;

                this._competitionRankingRepository.Update(ranking);
                updatedRanking = ranking;
            }

            rankings.Sort((x, y) =>
            {
                int primaryComparisonResult = y.Points.CompareTo(x.Points);

                if(primaryComparisonResult == 0)
                {
                    return x.Penalty.CompareTo(y.Penalty);
                }

                return primaryComparisonResult;
            });

            for(int i = 0; i < rankings.Count; i++)
            {
                rankings[i].RankOrder = i + 1;
            }

            this._competitionRankingRepository.UpdateRange(rankings);

            await this._dbContext.SaveChangesAsync();

            // Build the response with exercise attempts
            // Group attempts by exercise to get the count per exercise
            var exerciseAttemptsGrouped = attempts
                .GroupBy(a => a.ExerciseId)
                .Select(g => new Database.Responses.Competition.GroupExerciseAttemptResponse()
                {
                    GroupId = group.Id,
                    ExerciseId = g.Key,
                    Attempts = g.Count()
                }).ToList();

            var response = new CompetitionRankingResponse()
            {
                Id = updatedRanking.Id,
                Points = updatedRanking.Points,
                Penalty = updatedRanking.Penalty,
                RankOrder = updatedRanking.RankOrder,
                Group = new GroupResponse()
                {
                    Id = group.Id,
                    LeaderId = group.LeaderId,
                    Name = group.Name,
                    Users = group.Users.Select(u => new GenericUserInfoResponse()
                    {
                        Id = u.Id,
                        Email = u.Email!,
                        Department = null,
                        CreatedAt = u.CreatedAt,
                        ExercisesCreated = null,
                        JoinYear = u.JoinYear,
                        LastLoggedAt = u.LastLoggedAt,
                        Name = u.Name,
                        Ra = u.Name, // Using Name as Ra for now
                        Group = null,
                    }).ToList(),
                },
                ExerciseAttempts = exerciseAttemptsGrouped,
            };

            return response;
        }
    }
}
