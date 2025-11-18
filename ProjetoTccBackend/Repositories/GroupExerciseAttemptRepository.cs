using ProjetoTccBackend.Database;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Repositories.Interfaces;

namespace ProjetoTccBackend.Repositories
{
    public class GroupExerciseAttemptRepository : GenericRepository<GroupExerciseAttempt>, IGroupExerciseAttemptRepository
    {
        public GroupExerciseAttemptRepository(TccDbContext dbContext) : base(dbContext) { }


        /// <inheritdoc />
        public GroupExerciseAttempt? GetLastGroupCompetitionAttempt(int groupId, int competitionId)
        {
            var response = this._dbContext.GroupExerciseAttempts
                .Where(x => x.GroupId.Equals(groupId) && x.CompetitionId.Equals(competitionId))
                .OrderByDescending(x => x.SubmissionTime)
                .FirstOrDefault();

            return response;
        }
    }
}
