using ProjetoTccBackend.Models;

namespace ProjetoTccBackend.Repositories.Interfaces
{
    /// <summary>
    /// Defines repository operations specific to the <see cref="Competition"/> entity.
    /// Extends the generic repository interface with methods for handling competition-related data.
    /// </summary>
    public interface ICompetitionRepository : IGenericRepository<Competition>
    {
        /// <summary>
        /// Retrieves all questions associated with a specific competition.
        /// </summary>
        /// <param name="competitionId">The unique identifier of the competition.</param>
        /// <returns>A collection of <see cref="Question"/> objects related to the competition.</returns>
        Task<ICollection<Question>> GetCompetitionQuestions(int competitionId);


        /// <summary>
        /// Asynchronously retrieves a collection of competitions that are currently open.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of  <see
        /// cref="Competition"/> objects representing the open competitions. If no competitions are open,  the
        /// collection will be empty.</returns>
        Task<ICollection<Competition>> GetOpenCompetitionsAsync();
    }
}
