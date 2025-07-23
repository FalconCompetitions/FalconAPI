using ProjetoTccBackend.Database;
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
        public async Task UpdateRanking(Competition competition, Group group, GroupExerciseAttempt exerciseAttempt)
        {
            List<GroupExerciseAttempt> attempts = this._groupExerciseAttemptRepository.Find(
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
            }
            else
            {
                CompetitionRanking ranking = rankings[groupRankingIndex];

                ranking.Points = totalPoints;
                ranking.Penalty = totalPenalty;

                this._competitionRankingRepository.Update(ranking);
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
        }
    }
}
