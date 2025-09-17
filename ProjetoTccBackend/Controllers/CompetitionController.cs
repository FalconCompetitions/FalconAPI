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

        public CompetitionController(ICompetitionService competitionService, ILogger<CompetitionController> logger)
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
            Competition? existentCompetition = await this._competitionService.GetExistentCompetition();

            if(existentCompetition is null)
            {
                return NoContent();
            }

            return Ok(existentCompetition);
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
        public async Task<IActionResult> CreateNewCompetition([FromBody]CompetitionRequest request)
        {
            this._logger.LogInformation("Chegou");

            Competition? newCompetition = null;

            try
            {
                newCompetition = await this._competitionService.CreateCompetition(request);
            } catch(ExistentCompetitionException ex)
            {
                throw new FormException(new Dictionary<string, string>
                {
                    { "general", "Já existe uma competição marcada para a mesma data" }
                });
            }

            if(newCompetition is null)
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
        public async Task<IActionResult> UpdateCompetition(int id, [FromBody] UpdateCompetitionRequest request)
        {
            var updatedCompetition = await this._competitionService.UpdateCompetitionAsync(id, request);
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
            };

            return Ok(response);
        }
    }
}
