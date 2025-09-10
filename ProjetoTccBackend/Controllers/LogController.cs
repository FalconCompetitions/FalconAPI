using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjetoTccBackend.Database.Requests.Log;
using ProjetoTccBackend.Database.Responses.Global;
using ProjetoTccBackend.Database.Responses.Log;
using ProjetoTccBackend.Services.Interfaces;

namespace ProjetoTccBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogController : ControllerBase
    {
        private readonly ILogService _logService;

        public LogController(ILogService logService)
        {
            _logService = logService;
        }

        /// <summary>
        /// Cria um novo log.
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ProducesResponseType(typeof(LogResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateLog([FromBody] CreateLogRequest request)
        {
            var log = await _logService.CreateLogAsync(request);
            return Ok(log);
        }

        /// <summary>
        /// Retrieves a log entry by its unique identifier.
        /// </summary>
        /// <remarks>This endpoint is restricted to users with the "Admin" role. Ensure the caller has the
        /// appropriate authorization before invoking this method.</remarks>
        /// <param name="id">The unique identifier of the log entry to retrieve.</param>
        /// <returns>An <see cref="IActionResult"/> containing the log entry if found, with a status code of 200 (OK). Returns a
        /// status code of 404 (Not Found) if no log entry with the specified identifier exists.</returns>
        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(LogResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetLogById(int id)
        {
            var log = await _logService.GetLogByIdAsync(id);
            if (log == null) return NotFound(id);
            return Ok(log);
        }

        /// <summary>
        /// Retrieves a paginated list of logs, optionally filtered by a search term.
        /// </summary>
        /// <remarks>This endpoint is restricted to users with the "Admin" role. The response includes a
        /// paginated result  with metadata such as total count and current page. Use the <paramref name="search"/>
        /// parameter to  filter logs by a specific term.</remarks>
        /// <param name="page">The page number to retrieve. Must be 1 or greater. Defaults to 1.</param>
        /// <param name="pageSize">The number of logs to include per page. Must be 1 or greater. Defaults to 10.</param>
        /// <param name="search">An optional search term to filter the logs. If null or empty, no filtering is applied.</param>
        /// <returns>An <see cref="IActionResult"/> containing a <see cref="PagedResult{T}"/> of <see cref="LogResponse"/>
        /// objects  representing the logs for the specified page and page size.</returns>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<LogResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            var result = await _logService.GetLogsAsync(page, pageSize, search);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a paginated list of logs for a specific competition.
        /// </summary>
        /// <param name="competitionId">The competition ID to filter logs.</param>
        /// <param name="page">The page number to retrieve.</param>
        /// <param name="pageSize">The number of logs per page.</param>
        /// <param name="search">Optional search term for filtering logs.</param>
        [Authorize(Roles = "Admin")]
        [HttpGet("competition/{competitionId}/logs")]
        [ProducesResponseType(typeof(PagedResult<LogResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLogsByCompetition(int competitionId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            var result = await _logService.GetLogsByCompetitionAsync(competitionId, page, pageSize, search);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a paginated list of logs for a specific user.
        /// </summary>
        /// <param name="userId">The user ID to filter logs.</param>
        /// <param name="page">The page number to retrieve.</param>
        /// <param name="pageSize">The number of logs per page.</param>
        /// <param name="search">Optional search term for filtering logs.</param>
        [Authorize(Roles = "Admin")]
        [HttpGet("user/{userId}/logs")]
        [ProducesResponseType(typeof(PagedResult<LogResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLogsByUser(string userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            var result = await _logService.GetLogsByUserAsync(userId, page, pageSize, search);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a paginated list of logs for a specific group.
        /// </summary>
        /// <param name="groupId">The group ID to filter logs.</param>
        /// <param name="page">The page number to retrieve.</param>
        /// <param name="pageSize">The number of logs per page.</param>
        /// <param name="search">Optional search term for filtering logs.</param>
        [Authorize(Roles = "Admin")]
        [HttpGet("group/{groupId}/logs")]
        [ProducesResponseType(typeof(PagedResult<LogResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLogsByGroup(int groupId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            var result = await _logService.GetLogsByGroupAsync(groupId, page, pageSize, search);
            return Ok(result);
        }



    }
}
