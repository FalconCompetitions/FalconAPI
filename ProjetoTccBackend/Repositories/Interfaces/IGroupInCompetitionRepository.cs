using ProjetoTccBackend.Models;

namespace ProjetoTccBackend.Repositories.Interfaces
{
    public interface IGroupInCompetitionRepository : IGenericRepository<GroupInCompetition>
    {
        /// <summary>
        /// Retrieves the current valid competition registration for a group.
        /// A registration is considered valid if the current date is within the competition's date range.
        /// </summary>
        /// <param name="groupId">The ID of the group to retrieve the registration for.</param>
        /// <returns>The valid GroupInCompetition if found, otherwise null.</returns>
        Task<GroupInCompetition?> GetCurrentValidCompetitionByGroupIdAsync(int groupId);
    }
}
