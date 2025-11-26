using ProjetoTccBackend.Database.Requests.Log;
using ProjetoTccBackend.Database.Responses.Log;
using ProjetoTccBackend.Database.Responses.Global;
using ProjetoTccBackend.Models;

namespace ProjetoTccBackend.Services.Interfaces
{
    /// <summary>
    /// Service interface for managing log operations.
    /// </summary>
    public interface ILogService
    {
        /// <summary>
        /// Creates a new log entry.
        /// </summary>
        /// <param name="request">The request containing the log details.</param>
        /// <returns>The created log response.</returns>
        Task<LogResponse> CreateLogAsync(CreateLogRequest request);

        /// <summary>
        /// Retrieves a log entry by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the log entry.</param>
        /// <returns>The log response if found, otherwise null.</returns>
        Task<LogResponse?> GetLogByIdAsync(int id);

        /// <summary>
        /// Retrieves a paginated list of logs.
        /// </summary>
        /// <param name="page">The page number to retrieve.</param>
        /// <param name="pageSize">The number of logs per page.</param>
        /// <param name="search">Optional search term for filtering logs.</param>
        /// <returns>A paginated result of log responses.</returns>
        Task<PagedResult<LogResponse>> GetLogsAsync(int page, int pageSize, string? search = null);

        /// <summary>
        /// Retrieves a paginated list of logs for a specific competition.
        /// </summary>
        /// <param name="competitionId">The competition ID to filter logs.</param>
        /// <param name="page">The page number to retrieve.</param>
        /// <param name="pageSize">The number of logs per page.</param>
        /// <param name="search">Optional search term for filtering logs.</param>
        /// <returns>A paginated result of log responses.</returns>
        Task<PagedResult<LogResponse>> GetLogsByCompetitionAsync(int competitionId, int page, int pageSize, string? search = null);

        /// <summary>
        /// Retrieves a paginated list of logs for a specific user.
        /// </summary>
        /// <param name="userId">The user ID to filter logs.</param>
        /// <param name="page">The page number to retrieve.</param>
        /// <param name="pageSize">The number of logs per page.</param>
        /// <param name="search">Optional search term for filtering logs.</param>
        /// <returns>A paginated result of log responses.</returns>
        Task<PagedResult<LogResponse>> GetLogsByUserAsync(string userId, int page, int pageSize, string? search = null);

        /// <summary>
        /// Retrieves a paginated list of logs for a specific group.
        /// </summary>
        /// <param name="groupId">The group ID to filter logs.</param>
        /// <param name="page">The page number to retrieve.</param>
        /// <param name="pageSize">The number of logs per page.</param>
        /// <param name="search">Optional search term for filtering logs.</param>
        /// <returns>A paginated result of log responses.</returns>
        Task<PagedResult<LogResponse>> GetLogsByGroupAsync(int groupId, int page, int pageSize, string? search = null);
    }
}
