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

        public ExerciseController(ILogger<ExerciseController> logger, IExerciseService exerciseService)
        {
            this._logger = logger;
            this._exerciseService = exerciseService;
        }


        //[Authorize(Roles = "Admin,Teacher")]
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

        //[Authorize(Roles = "Admin,Teacher")]
        [HttpGet()]
        public async Task<IActionResult> GetExercises()
        {
            var exercises = await this._exerciseService.GetExercisesAsync();

            return Ok(exercises);
        }


        /// <summary>
        /// Creates a new exercise based on the provided request data.
        /// </summary>
        /// <remarks>This action is restricted to users with the "Admin" or "Teacher" roles.  The created
        /// exercise can be retrieved using the <see cref="GetExerciseById"/> action.</remarks>
        /// <param name="request">The request object containing the details of the exercise to be created.  This must include all required
        /// fields for creating an exercise.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.  Returns <see
        /// cref="CreatedAtActionResult"/> with the created exercise if successful,  or <see cref="BadRequestResult"/>
        /// if the creation fails.</returns>
        [Authorize(Roles = "Admin,Teacher")]
        [HttpPost()]
        public async Task<IActionResult> CreateNewExercise([FromBody] CreateExerciseRequest request)
        {
            Exercise? exercise = await this._exerciseService.CreateExerciseAsync(request);

            if (exercise == null)
            {
                this._logger.LogDebug("Exercise not created", new
                {
                    bodyContent = exercise
                });
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
        /// <remarks>This action requires the caller to be authenticated and authorized with either the
        /// "Admin" or "Teacher" role.</remarks>
        /// <param name="id">The unique identifier of the exercise to update.</param>
        /// <param name="request">The request object containing the updated exercise details.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [Authorize(Roles = "Admin,Teacher")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateExercise(int id, [FromBody] UpdateExerciseRequest request)
        {
            await this._exerciseService.UpdateExerciseAsync(id, request);
            return Ok();
        }


        /// <summary>
        /// Deletes an exercise with the specified identifier.
        /// </summary>
        /// <remarks>This action requires the caller to be authorized with the "Admin" or "Teacher"
        /// role.</remarks>
        /// <param name="id">The unique identifier of the exercise to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation. Returns <see cref="NoContentResult"/>
        /// if the deletion is successful.</returns>
        [Authorize(Roles = "Admin,Teacher")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExercise(int id)
        {
            await this._exerciseService.DeleteExerciseAsync(id);
            return NoContent();
        }
    }
}
