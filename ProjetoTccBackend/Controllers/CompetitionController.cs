using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjetoTccBackend.Database.Requests.Competition;
using ProjetoTccBackend.Database.Responses.Competition;
using ProjetoTccBackend.Exceptions;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Services.Interfaces;

namespace ProjetoTccBackend.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class CompetitionController : ControllerBase
    {
        private readonly ICompetitionService _competitionService;
        private readonly ILogger<CompetitionController> _logger;

        public CompetitionController(
            ICompetitionService competitionService,
            ILogger<CompetitionController> logger
        )
        {
            this._competitionService = competitionService;
            this._logger = logger;
        }

        /// <summary>
        /// Retrieves the existing competition.
        /// </summary>
        /// <returns>The existing <see cref="Competition"/> object if found, or <see cref="NoContentResult"/> if not found.</returns>
        /// <remarks>
        /// Exemplo de uso:
        /// <code>
        ///     GET /api/competition
        /// </code>
        /// </remarks>
        /// <response code="200">Returns the existing competition.</response>
        /// <response code="204">If no competition exists.</response>
        [HttpGet]
        public async Task<IActionResult> GetExistentCompetition()
        {
            Competition? existentCompetition =
                await this._competitionService.GetExistentCompetition();

            if (existentCompetition is null)
            {
                return NoContent();
            }

            return Ok(existentCompetition);
        }

        /// <summary>
        /// Retrieves a collection of competitions that were created as templates.
        /// </summary>
        /// <remarks>This method is accessible only to users with the "Admin" or "Teacher" roles. It
        /// returns a collection of competitions that are marked as templates, which can be used for creating new
        /// competitions based on predefined settings.</remarks>
        /// <returns>An <see cref="IActionResult"/> containing a collection of <see cref="CompetitionResponse"/> objects
        /// representing the template competitions. The response is returned with an HTTP 200 status code.</returns>
        [HttpGet("template")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetCreatedTemplateCompetitions()
        {
            ICollection<CompetitionResponse> response =
                await this._competitionService.GetCreatedTemplateCompetitions();

            return Ok(response);
        }

        /// <summary>
        /// Retrieves a list of competitions that currently have open inscriptions.
        /// </summary>
        /// <remarks>This method returns competitions where the inscription period is currently active.
        /// The response includes details such as the competition's name, description, duration, and other relevant
        /// metadata. The list is returned as a collection of competition responses.</remarks>
        /// <returns>An <see cref="IActionResult"/> containing a list of competitions with open inscriptions. The response is
        /// serialized as a collection of <see cref="CompetitionResponse"/> objects.</returns>
        /// <response code="200">Returns the competitions with open subscription</response>
        [HttpGet("open")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCompetitionsWithOpenInscriptions()
        {
            ICollection<Competition> res =
                await this._competitionService.GetOpenSubscriptionCompetitionsAsync();

            List<CompetitionResponse> response = res.Select(x => new CompetitionResponse()
                {
                    Id = x.Id,
                    Name = x.Name,
                    BlockSubmissions = x.BlockSubmissions,
                    CompetitionRankings = null,
                    Description = x.Description,
                    Duration = x.Duration,
                    StartInscriptions = x.StartInscriptions,
                    EndInscriptions = x.EndInscriptions,
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    ExerciseIds = [],
                    Exercises = [],
                    MaxExercises = x.MaxExercises,
                    MaxMembers = x.MaxMembers,
                    MaxSubmissionSize = x.MaxSubmissionSize,
                    Status = x.Status,
                    StopRanking = x.StopRanking,
                    SubmissionPenalty = x.SubmissionPenalty,
                })
                .ToList();

            return Ok(response);
        }

        /// <summary>
        /// Creates a new competition.
        /// </summary>
        /// <param name="request">The competition creation request containing start and end times.</param>
        /// <returns>The created <see cref="Competition"/> object in <see cref="CompetitionResponse"/> format.</returns>
        /// <remarks>
        /// Accessible only to users with the "Admin" role.<br/>
        /// Exemplo de request:
        /// <code>
        ///     POST /api/competition
        ///     {
        ///         "startTime": "2024-08-21T10:00:00",
        ///         "endTime": "2024-08-21T12:00:00"
        ///     }
        /// </code>
        /// </remarks>
        /// <response code="201">Returns the created competition.</response>
        /// <response code="400">If the request is invalid or a competition already exists for the same date.</response>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateNewCompetition([FromBody] CompetitionRequest request)
        {
            Competition? newCompetition = null;

            try
            {
                newCompetition = await this._competitionService.CreateCompetition(request);
            }
            catch (ExistentCompetitionException ex)
            {
                throw new FormException(
                    new Dictionary<string, string>
                    {
                        { "general", "Já existe uma competição marcada para a mesma data" },
                    }
                );
            }

            if (newCompetition is null)
            {
                throw new ErrorException("Não foi possível criar uma nova competição");
            }

            CompetitionResponse response = new CompetitionResponse()
            {
                Id = newCompetition.Id,
                SubmissionPenalty = newCompetition.SubmissionPenalty,
                StopRanking = newCompetition.StopRanking,
                MaxSubmissionSize = newCompetition.MaxSubmissionSize,
                MaxExercises = newCompetition.MaxExercises,
                BlockSubmissions = newCompetition.BlockSubmissions,
                EndInscriptions = newCompetition.EndInscriptions,
                EndTime = newCompetition.EndTime,
                ExerciseIds = request.ExerciseIds,
                StartInscriptions = newCompetition.StartInscriptions,
                StartTime = newCompetition.StartTime,
                Name = newCompetition.Name,
                Duration = newCompetition.Duration,
                Status = newCompetition.Status,
                Description = newCompetition.Description,
                MaxMembers = newCompetition.MaxMembers,
            };

            return CreatedAtAction(nameof(GetExistentCompetition), new { }, response);
        }

        /// <summary>
        /// Updates an existing competition.
        /// </summary>
        /// <param name="id">The unique identifier of the competition to update.</param>
        /// <param name="request">The update request containing new competition data.</param>
        /// <returns>The updated <see cref="Competition"/> object in <see cref="CompetitionResponse"/> format.</returns>
        /// <remarks>
        /// Accessible only to users with the "Admin" role.<br/>
        /// Exemplo de request:
        /// <code>
        ///     PUT /api/competition/1
        ///     {
        ///         "startTime": "2024-08-21T10:00:00",
        ///         "endTime": "2024-08-21T12:00:00"
        ///     }
        /// </code>
        /// </remarks>
        /// <response code="200">Returns the updated competition.</response>
        /// <response code="404">If the competition is not found.</response>
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCompetition(
            int id,
            [FromBody] UpdateCompetitionRequest request
        )
        {
            var updatedCompetition = await this._competitionService.UpdateCompetitionAsync(
                id,
                request
            );
            if (updatedCompetition == null)
            {
                return NotFound();
            }

            CompetitionResponse response = new CompetitionResponse()
            {
                Id = updatedCompetition.Id,
                SubmissionPenalty = updatedCompetition.SubmissionPenalty,
                StopRanking = updatedCompetition.StopRanking,
                MaxSubmissionSize = updatedCompetition.MaxSubmissionSize,
                MaxExercises = updatedCompetition.MaxExercises,
                BlockSubmissions = updatedCompetition.BlockSubmissions,
                EndInscriptions = updatedCompetition.EndInscriptions,
                EndTime = updatedCompetition.EndTime,
                ExerciseIds = request.ExerciseIds,
                StartInscriptions = updatedCompetition.StartInscriptions,
                StartTime = updatedCompetition.StartTime,
                Status = updatedCompetition.Status,
                Duration = updatedCompetition.Duration,
                Name = updatedCompetition.Name,
                Description = updatedCompetition.Description,
                MaxMembers = updatedCompetition.MaxMembers,
            };

            return Ok(response);
        }
    }
}
