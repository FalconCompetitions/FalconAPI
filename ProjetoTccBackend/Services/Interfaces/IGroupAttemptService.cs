using ProjetoTccBackend.Exceptions.Judge;
using ProjetoTccBackend.Database.Requests.Competition;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Database.Responses.Exercise;
using ProjetoTccBackend.Enums.Judge;

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



        /// <summary>
        /// Updates the judge response and acceptance status of a group exercise attempt.
        /// </summary>
        /// <remarks>This method retrieves the group exercise attempt by its identifier, updates its judge
        /// response  and acceptance status, and saves the changes to the database. The acceptance status is set to 
        /// <see langword="true"/> if <paramref name="newResponse"/> is <see cref="JudgeSubmissionResponse.Accepted"/>; 
        /// otherwise, it is set to <see langword="false"/>.</remarks>
        /// <param name="attemptId">The unique identifier of the group exercise attempt to update.</param>
        /// <param name="newResponse">The new judge response to assign to the group exercise attempt.</param>
        /// <returns><see langword="true"/> if the group exercise attempt was successfully updated;  otherwise, <see
        /// langword="false"/> if the specified attempt does not exist.</returns>
        Task<bool> ChangeGroupExerciseAttempt(int attemptId, JudgeSubmissionResponse newResponse);
    }
}
