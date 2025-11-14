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

        /// <summary>
        /// Retrieves all groups registered in a specific competition.
        /// </summary>
        /// <param name="competitionId">The ID of the competition.</param>
        /// <returns>A collection of GroupInCompetitionResponse objects.</returns>
        Task<ICollection<GroupInCompetitionResponse>> GetGroupsByCompetitionAsync(int competitionId);

        /// <summary>
        /// Unblocks a group in a specific competition, allowing them to submit exercises again.
        /// </summary>
        /// <param name="groupId">The ID of the group to unblock.</param>
        /// <param name="competitionId">The ID of the competition.</param>
        /// <returns>True if the group was successfully unblocked, false otherwise.</returns>
        Task<bool> UnblockGroupInCompetitionAsync(int groupId, int competitionId);
    }
}
