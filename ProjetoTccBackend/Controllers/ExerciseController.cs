using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjetoTccBackend.Database.Requests.Exercise;
using ProjetoTccBackend.Database.Responses.Exercise;
using ProjetoTccBackend.Database.Responses.Form;
using ProjetoTccBackend.Database.Responses.Global;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Services.Interfaces;

namespace ProjetoTccBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExerciseController : ControllerBase
    {
        private readonly IExerciseService _exerciseService;
        private readonly ILogger<ExerciseController> _logger;

        public ExerciseController(
            IExerciseService exerciseService,
            ILogger<ExerciseController> logger
        )
        {
            this._exerciseService = exerciseService;
            this._logger = logger;
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
            [FromQuery(Name = "exerciseType")] int? exerciseType = null
        )
        {
            var result = await this._exerciseService.GetExercisesAsync(
                page,
                pageSize,
                search,
                exerciseType
            );

            var response = new PagedResult<ExerciseResponse>()
            {
                Items = result.Items.Select(x => new ExerciseResponse()
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    ExerciseTypeId = x.ExerciseTypeId,
                    Inputs = x
                        .ExerciseInputs.Select(input => new ExerciseInputResponse()
                        {
                            Id = input.Id,
                            ExerciseId = input.ExerciseId,
                            Input = input.Input,
                        })
                        .ToList(),
                    Outputs = x
                        .ExerciseOutputs.Select(output => new ExerciseOutputResponse()
                        {
                            Id = output.Id,
                            Output = output.Output,
                            ExerciseId = output.ExerciseId,
                            ExerciseInputId = output.ExerciseInputId,
                        })
                        .ToList(),
                }),
                Page = result.Page,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount,
                TotalPages = result.TotalPages,
            };

            return Ok(response);
        }

        /// <summary>
        /// Creates a new exercise based on the provided request data.
        /// </summary>
        /// <param name="file">The attached file with details of the exercise.</param>
        /// <param name="requestMetadata">The request object containing the details of the exercise to be created. This must include all required fields for creating an exercise.</param>
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
        public async Task<IActionResult> CreateNewExercise(
            [FromForm(Name = "file")] IFormFile file,
            [FromForm(Name = "metadata")] string requestMetadata
        )
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(
                    new InvalidFormResponse() { Target = "file", Error = "Arquivo é obrigatório!" }
                );
            }

            if (String.IsNullOrEmpty(requestMetadata))
            {
                return BadRequest(
                    new InvalidFormResponse()
                    {
                        Target = "metadata",
                        Error = "Dados da requisição ausentes!",
                    }
                );
            }

            CreateExerciseRequest? request = null;

            try
            {
                request = JsonSerializer.Deserialize<CreateExerciseRequest>(requestMetadata);
            }
            catch (JsonException ex)
            {
                return BadRequest(
                    new InvalidFormResponse() { Target = "metadata", Error = ex.Message }
                );
            }

            this.TryValidateModel(request);

            if (this.ModelState.IsValid is false)
            {
                return ValidationProblem(this.ModelState);
            }

            Exercise? exercise = await this._exerciseService.CreateExerciseAsync(request, file);

            if (exercise == null)
            {
                this._logger.LogDebug("Exercise not created", new { bodyContent = exercise });
                return this.BadRequest();
            }

            return this.CreatedAtAction(
                nameof(this.GetExerciseById),
                new { id = exercise.Id },
                new ExerciseResponse()
                {
                    Id = exercise.Id,
                    Title = exercise.Title,
                    Description = exercise.Description,
                    Inputs = exercise
                        .ExerciseInputs.Select(x => new ExerciseInputResponse()
                        {
                            Id = x.Id,
                            ExerciseId = x.Id,
                            Input = x.Input,
                        })
                        .ToList(),
                    Outputs = exercise
                        .ExerciseOutputs.Select(x => new ExerciseOutputResponse()
                        {
                            Id = x.Id,
                            ExerciseId = x.ExerciseId,
                            Output = x.Output,
                            ExerciseInputId = x.ExerciseInputId,
                        })
                        .ToList(),
                }
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
            Exercise updatedExercise = await this._exerciseService.UpdateExerciseAsync(id, request);

            ExerciseResponse response = new ExerciseResponse()
            {
                Id = updatedExercise.Id,
                Title = updatedExercise.Title,
                Description = updatedExercise.Description,
                ExerciseTypeId = updatedExercise.ExerciseTypeId,
                Inputs = updatedExercise
                    .ExerciseInputs.Select(x => new ExerciseInputResponse()
                    {
                        Id = x.Id,
                        ExerciseId = x.Id,
                        Input = x.Input,
                    })
                    .ToList(),
                Outputs = updatedExercise
                    .ExerciseOutputs.Select(x => new ExerciseOutputResponse()
                    {
                        Id = x.Id,
                        Output = x.Output,
                        ExerciseId = x.ExerciseId,
                        ExerciseInputId = updatedExercise.Id,
                    })
                    .ToList(),
            };

            return Ok(response);
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
        /// <response code="200">If the deletion is successful.</response>
        /// <response code="404">If the exercise is not found.</response>
        [Authorize(Roles = "Admin,Teacher")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteExercise(int id)
        {
            await this._exerciseService.DeleteExerciseAsync(id);
            return Ok();
        }
    }
}
