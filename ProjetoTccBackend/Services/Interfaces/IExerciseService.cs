using ProjetoTccBackend.Database.Requests.Exercise;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Exceptions;
using ProjetoTccBackend.Database.Responses.Global;
using ProjetoTccBackend.Exceptions.AttachedFile;

namespace ProjetoTccBackend.Services.Interfaces
{
    public interface IExerciseService
    {
        /// <summary>
        /// Asynchronously creates a new exercise based on the provided request data and an attached file.
        /// </summary>
        /// <param name="request">The request object containing the details of the exercise to be created, including inputs and outputs.</param>
        /// <param name="file">The file to be attached to the exercise. The file is validated and persisted before associating with the exercise.</param>
        /// <returns>
        /// A <see cref="Task"/> that represents the asynchronous operation. The task result contains the created <see cref="Exercise"/> object,
        /// including its associated inputs, outputs, and attached file.
        /// </returns>
        /// <exception cref="InvalidAttachedFileException">
        /// Thrown when the provided file is invalid or has an unsupported format.
        /// </exception>
        /// <exception cref="ErrorException">
        /// Thrown when the exercise cannot be created in the Judge system.
        /// </exception>
        /// <remarks>
        /// This method performs the following steps:
        /// <list type="number">
        /// <item>Validates the attached file format. If invalid, throws <see cref="InvalidAttachedFileException"/>.</item>
        /// <item>Processes and saves the attached file, persisting its metadata in the database.</item>
        /// <item>Calls the Judge service to create the exercise in the Judge system and retrieves its UUID.</item>
        /// <item>Creates a new <see cref="Exercise"/> object, associating it with the attached file and the Judge UUID, and saves it to the repository.</item>
        /// <item>Processes the input and output data from the request, associating them with the created exercise.</item>
        /// <item>Saves the inputs and outputs to their respective repositories.</item>
        /// <item>Returns the created <see cref="Exercise"/> object, including its inputs, outputs, and attached file.</item>
        /// </list>
        /// </remarks>
        Task<Exercise> CreateExerciseAsync(CreateExerciseRequest request, IFormFile file);


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
        /// Retrieves a paginated list of exercises asynchronously, with an optional search term.
        /// </summary>
        /// <param name="page">The page number to retrieve (1-based index).</param>
        /// <param name="pageSize">The number of exercises per page.</param>
        /// <param name="search">An optional search term to filter exercises by name or description.</param>
        /// <param name="exerciseTypeId">An optional exercise type to filter exercises by categories.</param>
        /// 
        /// <returns>
        /// A Task that represents the asynchronous operation. The task result contains a <see cref="PagedResult{Exercise}"/> object
        /// with the paginated list of exercises and additional pagination information.
        /// </returns>
        Task<PagedResult<Exercise>> GetExercisesAsync(int page, int pageSize, string? search = null, int? exerciseTypeId = null);


        /// <summary>
        /// Updates an existing exercise and its inputs and outputs with the provided data.
        /// </summary>
        /// <param name="id">The ID of the exercise to update.</param>
        /// <param name="request">The data to update the exercise with.</param>
        /// <returns>The updated <see cref="Exercise"/> object</returns>
        /// <exception cref="ErrorException">Thrown when the exercise with the specified ID is not found.</exception>
        Task<Exercise> UpdateExerciseAsync(int id, IFormFile file, UpdateExerciseRequest request);


        /// <summary>
        /// Deletes an exercise by its ID.
        /// </summary>
        /// <param name="id">The ID of the exercise to delete.</param>
        /// <exception cref="ErrorException">Thrown when the exercise with the specified ID is not found.</exception>
        Task DeleteExerciseAsync(int id);
    }
}
