using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjetoTccBackend.Database.Requests.Log;
using ProjetoTccBackend.Database.Responses.Global;
using ProjetoTccBackend.Database.Responses.Log;
using ProjetoTccBackend.Services.Interfaces;

namespace ProjetoTccBackend.Controllers
{
    /// <summary>
    /// Controller responsible for managing log operations.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class LogController : ControllerBase
    {
        private readonly ILogService _logService;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogController"/> class.
        /// </summary>
        /// <param name="logService">The service responsible for log operations.</param>
        public LogController(ILogService logService)
        {
            _logService = logService;
        }

        /// <summary>
        /// Creates a new log entry.
        /// </summary>
        /// <param name="request">The request containing the log details.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing the created log entry.
        /// </returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/Log
        ///     {
        ///         "message": "User performed action",
        ///         "level": "Info",
        ///         "userId": "user123",
        ///         "competitionId": 1
        ///     }
        ///
        /// Sample response:
        ///
        ///     {
        ///         "id": 1,
        ///         "message": "User performed action",
        ///         "level": "Info",
        ///         "userId": "user123",
        ///         "competitionId": 1,
        ///         "createdAt": "2024-01-15T10:30:00Z"
        ///     }
        /// </remarks>
        /// <response code="200">Returns the created log entry</response>
        /// <response code="400">If the request is invalid</response>
        /// <response code="401">If the user is not authenticated or authorized</response>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ProducesResponseType(typeof(LogResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
        /// <param name="page">The page number to retrieve. Default is 1.</param>
        /// <param name="pageSize">The number of logs per page. Default is 10.</param>
        /// <param name="search">Optional search term for filtering logs.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing a paginated list of logs for the specified competition.
        /// </returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/Log/competition/1/logs?page=1&pageSize=10&search=error
        ///
        /// Sample response:
        ///
        ///     {
        ///         "items": [...],
        ///         "page": 1,
        ///         "pageSize": 10,
        ///         "totalCount": 50,
        ///         "totalPages": 5
        ///     }
        /// </remarks>
        /// <response code="200">Returns the paginated list of logs for the competition</response>
        /// <response code="401">If the user is not authenticated or authorized</response>
        [Authorize(Roles = "Admin")]
        [HttpGet("competition/{competitionId}/logs")]
        [ProducesResponseType(typeof(PagedResult<LogResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetLogsByCompetition(int competitionId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            var result = await _logService.GetLogsByCompetitionAsync(competitionId, page, pageSize, search);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a paginated list of logs for a specific user.
        /// </summary>
        /// <param name="userId">The user ID to filter logs.</param>
        /// <param name="page">The page number to retrieve. Default is 1.</param>
        /// <param name="pageSize">The number of logs per page. Default is 10.</param>
        /// <param name="search">Optional search term for filtering logs.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing a paginated list of logs for the specified user.
        /// </returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/Log/user/user123/logs?page=1&pageSize=10&search=login
        ///
        /// Sample response:
        ///
        ///     {
        ///         "items": [...],
        ///         "page": 1,
        ///         "pageSize": 10,
        ///         "totalCount": 25,
        ///         "totalPages": 3
        ///     }
        /// </remarks>
        /// <response code="200">Returns the paginated list of logs for the user</response>
        /// <response code="401">If the user is not authenticated or authorized</response>
        [Authorize(Roles = "Admin")]
        [HttpGet("user/{userId}/logs")]
        [ProducesResponseType(typeof(PagedResult<LogResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetLogsByUser(string userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            var result = await _logService.GetLogsByUserAsync(userId, page, pageSize, search);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a paginated list of logs for a specific group.
        /// </summary>
        /// <param name="groupId">The group ID to filter logs.</param>
        /// <param name="page">The page number to retrieve. Default is 1.</param>
        /// <param name="pageSize">The number of logs per page. Default is 10.</param>
        /// <param name="search">Optional search term for filtering logs.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing a paginated list of logs for the specified group.
        /// </returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/Log/group/1/logs?page=1&pageSize=10&search=submission
        ///
        /// Sample response:
        ///
        ///     {
        ///         "items": [...],
        ///         "page": 1,
        ///         "pageSize": 10,
        ///         "totalCount": 15,
        ///         "totalPages": 2
        ///     }
        /// </remarks>
        /// <response code="200">Returns the paginated list of logs for the group</response>
        /// <response code="401">If the user is not authenticated or authorized</response>
        [Authorize(Roles = "Admin")]
        [HttpGet("group/{groupId}/logs")]
        [ProducesResponseType(typeof(PagedResult<LogResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetLogsByGroup(int groupId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            var result = await _logService.GetLogsByGroupAsync(groupId, page, pageSize, search);
            return Ok(result);
        }



    }
}
