using ProjetoTccBackend.Models;

namespace ProjetoTccBackend.Repositories.Interfaces
{
    public interface IGroupExerciseAttemptRepository : IGenericRepository<GroupExerciseAttempt>
    {
        /// <summary>
        /// Retrieves the most recent group exercise attempt for a given group.
        /// </summary>
        /// <param name="groupId">The ID of the group for which to retrieve the last attempt.</param>
        /// <param name="competitionId">The ID of the competition for which to retrieve the last attempt.</param>
        /// <returns>The most recent GroupExerciseAttemptResponse for the specified group, or null if no attempts exist.</returns>
        GroupExerciseAttempt? GetLastGroupCompetitionAttempt(int groupId, int competitionId);

        /// <summary>
        /// Checks if a group has already accepted (solved) a specific exercise in a competition.
        /// </summary>
        /// <param name="groupId">The ID of the group.</param>
        /// <param name="competitionId">The ID of the competition.</param>
        /// <param name="exerciseId">The ID of the exercise.</param>
        /// <returns>True if the group has already solved the exercise, false otherwise.</returns>
        bool HasGroupAcceptedExercise(int groupId, int competitionId, int exerciseId);
    }
}
