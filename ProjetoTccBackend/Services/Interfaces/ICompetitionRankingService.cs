using ProjetoTccBackend.Database.Responses.Competition;
using ProjetoTccBackend.Models;

namespace ProjetoTccBackend.Services.Interfaces
{
    public interface ICompetitionRankingService
    {
        /// <summary>
        /// Updates the ranking of a group after an exercise attempt
        /// </summary>
        /// <param name="competition">The competition the exercise attempt belongs to</param>
        /// <param name="group">The group the exercise attempt belongs to</param>
        /// <param name="exerciseAttempt">The exercise attempt that was submitted</param>
        /// <returns>The updated competition ranking response for the group</returns>
        Task<CompetitionRankingResponse> UpdateRanking(Competition competition, Group group, Models.GroupExerciseAttempt exerciseAttempt);
    }
}
