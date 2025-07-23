using ProjetoTccBackend.Exceptions.Judge;
using ProjetoTccBackend.Database.Requests.Competition;
using ProjetoTccBackend.Database.Responses.Competition;
using ProjetoTccBackend.Models;

namespace ProjetoTccBackend.Services.Interfaces
{
    public interface IGroupAttemptService
    {
        /// <summary>
        /// Submit a group exercise attempt to the judge API and updates the ranking of the current competition
        /// </summary>
        /// <param name="currentCompetition">The current competition</param>
        /// <param name="request">The group exercise attempt request</param>
        /// <returns>The exercise submission response from the judge API</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user is not authorized to submit an exercise attempt</exception>
        /// <exception cref="JudgeException">Thrown when the judge API returns an error</exception>
        Task<ExerciseSubmissionResponse> SubmitExerciseAttempt(Competition currentCompetition, GroupExerciseAttemptRequest request);
    }
}
