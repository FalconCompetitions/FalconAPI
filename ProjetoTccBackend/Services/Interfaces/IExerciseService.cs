using ProjetoTccBackend.Database.Requests.Exercise;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Exceptions;

namespace ProjetoTccBackend.Services.Interfaces
{
    public interface IExerciseService
    {
        /// <summary>
        /// Creates a new exercise asynchronously based on the provided request data.
        /// </summary>
        /// <param name="request">The request object containing the details of the exercise to be created, including inputs and outputs.</param>
        /// <returns>
        /// A Task that represents the asynchronous operation. The task result contains the created <see cref="Exercise"/> object.
        /// </returns>
        /// <exception cref="ErrorException">
        /// Thrown when the exercise cannot be created in the Judge system.
        /// </exception>
        /// <remarks>
        /// This method performs the following steps:
        /// 1. Calls the Judge service to create the exercise in the Judge system and retrieves its UUID.
        /// 2. Throws an exception if the Judge system fails to create the exercise.
        /// 3. Creates a new <see cref="Exercise"/> object and saves it to the repository.
        /// 4. Processes the input and output data from the request, associating them with the created exercise.
        /// 5. Saves the inputs and outputs to their respective repositories.
        /// </remarks>
        Task<Exercise> CreateExerciseAsync(CreateExerciseRequest request);


        /// <summary>
        /// Retrieves the specific exercise by its ID asynchronously.
        /// </summary>
        /// <returns>A object of <see cref="Exercise"/></returns>
        Task<Exercise?> GetExerciseByIdAsync(int id);


        /// <summary>
        /// Retrieves a list of all exercises asynchronously.
        /// </summary>
        /// <returns>A list of users.</returns>
        Task<List<Exercise>> GetExercisesAsync();


        /// <summary>
        /// Updates an existing exercise and its inputs and outputs with the provided data.
        /// </summary>
        /// <param name="id">The ID of the exercise to update.</param>
        /// <param name="request">The data to update the exercise with.</param>
        /// <exception cref="ErrorException">Thrown when the exercise with the specified ID is not found.</exception>
        Task UpdateExerciseAsync(int id, UpdateExerciseRequest request);


        /// <summary>
        /// Deletes an exercise by its ID.
        /// </summary>
        /// <param name="id">The ID of the exercise to delete.</param>
        /// <exception cref="ErrorException">Thrown when the exercise with the specified ID is not found.</exception>
        Task DeleteExerciseAsync(int id);
    }
}
