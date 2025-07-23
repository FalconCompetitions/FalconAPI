using ProjetoTccBackend.Database.Requests.Competition;
using ProjetoTccBackend.Models;

namespace ProjetoTccBackend.Services.Interfaces
{
    /// <summary>
    /// Service interface for managing competitions.
    /// </summary>
    public interface ICompetitionService
    {
        /// <summary>
        /// Creates a new competition based on the provided request data.
        /// </summary>
        /// <param name="request">The competition creation request containing start and end times.</param>
        /// <returns>The created <see cref="Competition"/> object.</returns>
        Task<Competition> CreateCompetition(CompetitionRequest request);

        /// <summary>
        /// Retrieves the existing competition.
        /// </summary>
        /// <returns>The existing <see cref="Competition"/> object.</returns>
        Task<Competition> GetExistentCompetition();

        /// <summary>
        /// Retrieves the current competition based on the system's current date and time.
        /// 
        /// This method queries the competition repository to find a competition where the current date and time falls within its start and end time.
        /// 
        /// Returns the current competition if found, otherwise returns null.
        /// </summary>
        Task<Competition?> GetCurrentCompetition();

        Task<Question> CreateGroupQuestion(string userId, int groupId, CreateGroupQuestionRequest request);
    
        

    }
}
