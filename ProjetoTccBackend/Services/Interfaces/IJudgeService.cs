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


        /// <summary>
        /// Asynchronously retrieves a collection of exercises.
        /// </summary>
        /// <remarks>This method is intended to fetch exercises from a data source. The caller should
        /// await the returned task to ensure the operation completes before accessing the result.</remarks>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of  <see
        /// cref="Exercise"/> objects representing the exercises. The collection will be empty if no exercises are
        /// available.</returns>
        Task<ICollection<Exercise>> GetExercisesAsync();

        /// <summary>
        /// Sends a group exercise attempt to the judge API.
        /// </summary>
        /// <param name="request">The group exercise attempt request.</param>
        /// <returns>The result of the submission as a JudgeSubmissionResponseEnum.</returns>
        Task<JudgeSubmissionResponseEnum> SendGroupExerciseAttempt(GroupExerciseAttemptWorkerRequest request);

        /// <summary>
        /// Updates the specified exercise in the judge service asynchronously.
        /// </summary>
        /// <remarks>This method performs an asynchronous operation to update the details of an existing
        /// exercise. Ensure that the provided <paramref name="exercise"/> object and the judge service contains valid and up-to-date
        /// information.</remarks>
        /// <param name="exercise">The exercise to update. Must not be <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the update was successful; otherwise, <see langword="false"/>.</returns>
        Task<bool> UpdateExerciseAsync(Exercise exercise);



        /// <summary>
        /// Authenticates a judge by generating a token and sending it to the authentication endpoint.
        /// </summary>
        /// <remarks>This method generates a token using the token service and sends it to the
        /// authentication endpoint to retrieve an access token. If the authentication fails or an exception occurs, the
        /// method logs the error and returns <see langword="null"/>.</remarks>
        /// <returns>A <see cref="string"/> containing the access token if authentication is successful; otherwise,  <see
        /// langword="null"/>.</returns>
        Task<string?> AuthenticateJudge();


        /// <summary>
        /// Retrieves a valid judge token, either from the cache or by authenticating a new one.
        /// </summary>
        /// <remarks>This method first attempts to retrieve a cached token and validates its validity.  If
        /// the cached token is invalid or unavailable, it authenticates a new token and caches it for future
        /// use.</remarks>
        /// <returns>A valid judge token as a string, or <see langword="null"/> if authentication fails.</returns>
        Task<string?> FetchJudgeToken();
    }
}
