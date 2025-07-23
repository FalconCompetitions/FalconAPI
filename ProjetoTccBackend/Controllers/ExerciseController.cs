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


        //[Authorize(Roles = "Admin,Teacher")]
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

            return this.CreatedAtAction(nameof(this.GetExerciseById), new { id = exercise.Id }, exercise);
        }
    }
}
