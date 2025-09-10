using ProjetoTccBackend.Database.Requests.Log;
using ProjetoTccBackend.Database.Responses.Log;
using ProjetoTccBackend.Database.Responses.Global;
using ProjetoTccBackend.Models;

namespace ProjetoTccBackend.Services.Interfaces
{
    public interface ILogService
    {
        Task<LogResponse> CreateLogAsync(CreateLogRequest request);
        Task<LogResponse?> GetLogByIdAsync(int id);
        Task<PagedResult<LogResponse>> GetLogsAsync(int page, int pageSize, string? search = null);
        Task<PagedResult<LogResponse>> GetLogsByCompetitionAsync(int competitionId, int page, int pageSize, string? search = null);
        Task<PagedResult<LogResponse>> GetLogsByUserAsync(string userId, int page, int pageSize, string? search = null);
        Task<PagedResult<LogResponse>> GetLogsByGroupAsync(int groupId, int page, int pageSize, string? search = null);
    }
}
