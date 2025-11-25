using Microsoft.EntityFrameworkCore;
using ProjetoTccBackend.Database;
using ProjetoTccBackend.Database.Requests.Log;
using ProjetoTccBackend.Database.Responses.Global;
using ProjetoTccBackend.Database.Responses.Log;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Repositories.Interfaces;
using ProjetoTccBackend.Services.Interfaces;

namespace ProjetoTccBackend.Services
{
    public class LogService : ILogService
    {
        private readonly ILogRepository _logRepository;
        private readonly TccDbContext _dbContext;

        public LogService(ILogRepository logRepository, TccDbContext dbContext)
        {
            _logRepository = logRepository;
            _dbContext = dbContext;
        }

        public async Task<LogResponse> CreateLogAsync(CreateLogRequest request)
        {
            var log = new Log
            {
                ActionType = request.ActionType,
                ActionTime = DateTime.UtcNow,
                IpAddress = request.IpAddress,
                UserId = request.UserId,
                GroupId = request.GroupId,
                CompetitionId = request.CompetitionId
            };
            _logRepository.Add(log);
            _dbContext.SaveChanges();
            return await Task.FromResult(ToResponse(log));
        }

        public async Task<LogResponse?> GetLogByIdAsync(int id)
        {
            var log = _logRepository.GetById(id);
            if (log == null) return null;
            return await Task.FromResult(ToResponse(log));
        }

        public async Task<PagedResult<LogResponse>> GetLogsAsync(int page, int pageSize, string? search = null)
        {
            var query = _logRepository.GetAll().AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(l => l.IpAddress.Contains(search));
            }
            int totalCount = query.Count();
            var items = query.OrderByDescending(l => l.ActionTime).Skip((page - 1) * pageSize).Take(pageSize).ToList();
            var responses = items.Select(ToResponse).ToList();
            return await Task.FromResult(new PagedResult<LogResponse>
            {
                Items = responses,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        public async Task<PagedResult<LogResponse>> GetLogsByCompetitionAsync(int competitionId, int page, int pageSize, string? search = null)
        {
            var query = _logRepository.GetByCompetitionId(competitionId).AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(l => l.IpAddress.Contains(search));
            }
            int totalCount = query.Count();
            var items = query.OrderByDescending(l => l.ActionTime).Skip((page - 1) * pageSize).Take(pageSize).ToList();
            var responses = items.Select(ToResponse).ToList();
            return await Task.FromResult(new PagedResult<LogResponse>
            {
                Items = responses,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        public async Task<PagedResult<LogResponse>> GetLogsByUserAsync(string userId, int page, int pageSize, string? search = null)
        {
            var query = _logRepository.GetByUserId(userId).AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(l => l.IpAddress.Contains(search));
            }
            int totalCount = query.Count();
            var items = query.OrderByDescending(l => l.ActionTime).Skip((page - 1) * pageSize).Take(pageSize).ToList();
            var responses = items.Select(ToResponse).ToList();
            return await Task.FromResult(new PagedResult<LogResponse>
            {
                Items = responses,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        public async Task<PagedResult<LogResponse>> GetLogsByGroupAsync(int groupId, int page, int pageSize, string? search = null)
        {
            var query = _logRepository.GetByGroupId(groupId).AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(l => l.IpAddress.Contains(search));
            }
            int totalCount = query.Count();
            var items = query.OrderByDescending(l => l.ActionTime).Skip((page - 1) * pageSize).Take(pageSize).ToList();
            var responses = items.Select(ToResponse).ToList();
            return await Task.FromResult(new PagedResult<LogResponse>
            {
                Items = responses,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        private static LogResponse ToResponse(Log log)
        {
            return new LogResponse
            {
                Id = log.Id,
                ActionType = log.ActionType,
                ActionTime = log.ActionTime,
                IpAddress = log.IpAddress,
                UserId = log.UserId,
                GroupId = log.GroupId,
                CompetitionId = log.CompetitionId
            };
        }
    }
}
