using ProjetoTccBackend.Database.Responses.Competition;

namespace ProjetoTccBackend.Services.Interfaces
{
    /// <summary>
    /// Service responsible for managing group-in-competition operations.
    /// </summary>
    public interface IGroupInCompetitionService
    {
        /// <summary>
        /// Retrieves the current valid competition registration for a group.
        /// A registration is considered valid if the current date is within the competition's date range.
        /// </summary>
        /// <param name="groupId">The ID of the group to retrieve the registration for.</param>
        /// <returns>The valid GroupInCompetitionResponse if found, otherwise null.</returns>
        Task<GroupInCompetitionResponse?> GetCurrentValidCompetitionByGroupIdAsync(int groupId);

        /// <summary>
        /// Checks if a group is blocked from participating in a specific competition.
        /// </summary>
        /// <param name="groupId">The ID of the group to check.</param>
        /// <param name="competitionId">The ID of the competition to check.</param>
        /// <returns>True if the group is blocked, false otherwise.</returns>
        Task<bool> IsGroupBlockedInCompetitionAsync(int groupId, int competitionId);
    }
}
