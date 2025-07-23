using ProjetoTccBackend.Database.Requests.Competition;
using ProjetoTccBackend.Database.Requests.Exercise;
using ProjetoTccBackend.Models;
using JudgeSubmissionResponseEnum = ProjetoTccBackend.Enums.Judge.JudgeSubmissionResponse;

namespace ProjetoTccBackend.Services.Interfaces
{
    public interface IJudgeService
    {
        /// <summary>
        /// Creates a new exercise in the Judge system using the provided exercise request data.
        /// </summary>
        /// <param name="exerciseRequest">The request object containing the details of the exercise to be created.</param>
        /// <returns>
        /// A Task that represents the asynchronous operation. The task result contains the UUID of the created exercise
        /// in the Judge system if the operation is successful; otherwise, null.
        /// </returns>
        /// <remarks>
        /// This method sends a POST request to the Judge API to create a new exercise. The payload includes the exercise's
        /// title, description, input, and output data. If the exercise is successfully created, the method returns the
        /// UUID of the created exercise. If the creation fails or the response is invalid, the method returns null.
        /// </remarks>
        Task<string?> CreateJudgeExerciseAsync(CreateExerciseRequest exerciseRequest);

        /// <summary>
        /// Retrieves an exercise by its unique judge UUID.
        /// </summary>
        /// <param name="judgeUuid">The unique UUID of the exercise to retrieve.</param>
        /// <returns>The exercise associated with the provided UUID, or null if not found.</returns>
        Task<Exercise?> GetExerciseByUuidAsync(string judgeUuid);

        Task<ICollection<Exercise>> GetExercisesAsync();

        /// <summary>
        /// Sends a group exercise attempt to the judge API.
        /// </summary>
        /// <param name="request">The group exercise attempt request.</param>
        /// <returns>The result of the submission as a JudgeSubmissionResponseEnum.</returns>
        Task<JudgeSubmissionResponseEnum> SendGroupExerciseAttempt(GroupExerciseAttemptRequest request);
    }
}
