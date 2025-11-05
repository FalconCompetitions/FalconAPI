using Microsoft.EntityFrameworkCore;
using ProjetoTccBackend.Database;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Repositories.Interfaces;

namespace ProjetoTccBackend.Repositories
{
    public class GroupInCompetitionRepository : GenericRepository<GroupInCompetition>, IGroupInCompetitionRepository
    {
        public GroupInCompetitionRepository(TccDbContext dbContext) : base(dbContext)
        {
            
        }

        /// <summary>
        /// Retrieves the current valid competition registration for a group.
        /// A registration is considered valid if the current date is within the competition's date range.
        /// </summary>
        /// <param name="groupId">The ID of the group to retrieve the registration for.</param>
        /// <returns>The valid GroupInCompetition if found, otherwise null.</returns>
        public async Task<GroupInCompetition?> GetCurrentValidCompetitionByGroupIdAsync(int groupId)
        {
            DateTime now = DateTime.UtcNow;

            return await _dbContext.GroupsInCompetitions
                .Include(gic => gic.Competition)
                    .ThenInclude(c => c.ExercisesInCompetition)
                .Include(gic => gic.Competition)
                    .ThenInclude(c => c.CompetitionRankings)
                .Include(gic => gic.Group)
                    .ThenInclude(g => g.Users)
                .Where(gic => 
                    gic.GroupId == groupId && 
                    !gic.Blocked &&
                    gic.Competition.StartTime <= now &&
                    (gic.Competition.EndTime == null || gic.Competition.EndTime >= now))
                .OrderByDescending(gic => gic.Competition.StartTime)
                .FirstOrDefaultAsync();
        }
    }
}
