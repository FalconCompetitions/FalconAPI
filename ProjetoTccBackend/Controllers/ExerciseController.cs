using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjetoTccBackend.Database.Requests.Exercise;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Services.Interfaces;

namespace ProjetoTccBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExerciseController : ControllerBase
    {
        private readonly ILogger<ExerciseController> _logger;
        private readonly IExerciseService _exerciseService;

        public ExerciseController(
            ILogger<ExerciseController> logger,
            IExerciseService exerciseService
        )
        {
            this._logger = logger;
            this._exerciseService = exerciseService;
        }

        /// <summary>
        /// Retrieves a specific exercise by its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the exercise to retrieve.</param>
        /// <returns>The exercise if found, or <see cref="NotFoundResult"/> if not found.</returns>
        /// <remarks>
        /// Exemplo de uso:
        /// <code>
        ///     GET /api/exercise/1
        /// </code>
        /// </remarks>
        /// <response code="200">Returns the exercise details.</response>
        /// <response code="404">If the exercise is not found.</response>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetExerciseById(int id)
        {
            Exercise? exercise = await this._exerciseService.GetExerciseByIdAsync(id);

            if (exercise == null)
            {
                return NotFound(id);
            }

            return Ok(exercise);
        }

        /// <summary>
        /// Retrieves a paginated list of exercises.
        /// </summary>
        /// <param name="page">The page number to retrieve. Default is 1.</param>
        /// <param name="pageSize">The number of exercises per page. Default is 10.</param>
        /// <param name="search">Optional. A search term to filter exercises by title or description.</param>
        /// <returns>An <see cref="IActionResult"/> containing a paginated list of exercises.</returns>
        /// <remarks>
        /// Exemplo de uso:
        /// <code>
        ///     GET /api/exercise?page=1&pageSize=10&search=algoritmo
        /// </code>
        /// </remarks>
        /// <response code="200">Returns the paginated list of exercises.</response>
        [HttpGet()]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetExercises(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] int? exerciseType = null
        )
        {
            var result = await this._exerciseService.GetExercisesAsync(page, pageSize, search);
            return Ok(result);
        }

        /// <summary>
        /// Creates a new exercise based on the provided request data.
        /// </summary>
        /// <param name="request">The request object containing the details of the exercise to be created. This must include all required fields for creating an exercise.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation. Returns <see cref="CreatedAtActionResult"/> with the created exercise if successful, or <see cref="BadRequestResult"/> if the creation fails.</returns>
        /// <remarks>
        /// This action is restricted to users with the "Admin" or "Teacher" roles.<br/>
        /// Exemplo de request:
        /// <code>
        ///     POST /api/exercise
        ///     {
        ///         "exerciseTypeId": 1,
        ///         "title": "Soma de Números",
        ///         "description": "Some dois números inteiros.",
        ///         "estimatedTime": "00:30:00",
        ///         "inputs": [...],
        ///         "outputs": [...]
        ///     }
        /// </code>
        /// </remarks>
        /// <response code="201">Returns the created exercise.</response>
        /// <response code="400">If the request is invalid.</response>
        [Authorize(Roles = "Admin,Teacher")]
        [HttpPost()]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateNewExercise([FromBody] CreateExerciseRequest request)
        {
            Exercise? exercise = await this._exerciseService.CreateExerciseAsync(request);

            if (exercise == null)
            {
                this._logger.LogDebug("Exercise not created", new { bodyContent = exercise });
                return this.BadRequest();
            }

            return this.CreatedAtAction(
                nameof(this.GetExerciseById),
                new { id = exercise.Id },
                exercise
            );
        }

        /// <summary>
        /// Updates an existing exercise with the specified ID using the provided update request.
        /// </summary>
        /// <param name="id">The unique identifier of the exercise to update.</param>
        /// <param name="request">The request object containing the updated exercise details.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        /// <remarks>
        /// This action requires the caller to be authenticated and authorized with either the "Admin" or "Teacher" role.<br/>
        /// Exemplo de request:
        /// <code>
        ///     PUT /api/exercise/1
        ///     {
        ///         "id": 1,
        ///         "exerciseTypeId": 1,
        ///         "title": "Nova Soma",
        ///         "description": "Atualize a soma de dois números.",
        ///         "estimatedTime": "00:20:00",
        ///         "inputs": [...],
        ///         "outputs": [...]
        ///     }
        /// </code>
        /// </remarks>
        /// <response code="200">If the update is successful.</response>
        /// <response code="404">If the exercise is not found.</response>
        [Authorize(Roles = "Admin,Teacher")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateExercise(
            int id,
            [FromBody] UpdateExerciseRequest request
        )
        {
            await this._exerciseService.UpdateExerciseAsync(id, request);
            return Ok();
        }

        /// <summary>
        /// Deletes an exercise with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the exercise to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation. Returns <see cref="NoContentResult"/> if the deletion is successful.</returns>
        /// <remarks>
        /// This action requires the caller to be authorized with the "Admin" or "Teacher" role.<br/>
        /// Exemplo de uso:
        /// <code>
        ///     DELETE /api/exercise/1
        /// </code>
        /// </remarks>
        /// <response code="204">If the deletion is successful.</response>
        /// <response code="404">If the exercise is not found.</response>
        [Authorize(Roles = "Admin,Teacher")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteExercise(int id)
        {
            await this._exerciseService.DeleteExerciseAsync(id);
            return NoContent();
        }
    }
}
