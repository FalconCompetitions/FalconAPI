using Microsoft.EntityFrameworkCore;
using ProjetoTccBackend.Database;
using ProjetoTccBackend.Database.Requests.Log;
using ProjetoTccBackend.Database.Responses.Global;
using ProjetoTccBackend.Database.Responses.Log;
using ProjetoTccBackend.Enums.Log;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Repositories.Interfaces;
using ProjetoTccBackend.Services.Interfaces;

namespace ProjetoTccBackend.Services
{
    /// <summary>
    /// Service responsible for managing log operations.
    /// </summary>
    public class LogService : ILogService
    {
        private readonly ILogRepository _logRepository;
        private readonly TccDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogService"/> class.
        /// </summary>
        /// <param name="logRepository">The repository for log data access.</param>
        /// <param name="dbContext">The database context.</param>
        public LogService(ILogRepository logRepository, TccDbContext dbContext)
        {
            _logRepository = logRepository;
            _dbContext = dbContext;
        }

        /// <inheritdoc />
        public async Task<LogResponse> CreateLogAsync(CreateLogRequest request)
        {
            var log = new Log
            {
                ActionType = request.ActionType,
                ActionTime = DateTime.UtcNow,
                IpAddress = request.IpAddress,
                UserId = request.UserId,
                GroupId = request.GroupId,
                CompetitionId = request.CompetitionId,
            };
            this._logRepository.Add(log);
            this._dbContext.SaveChanges();

            // Fetch the created log with navigation properties
            var createdLog = await this
                ._logRepository.Query()
                .Include(l => l.User)
                .Include(l => l.Group)
                .FirstOrDefaultAsync(l => l.Id == log.Id);

            return ToResponse(createdLog ?? log);
        }

        /// <inheritdoc />
        public async Task<LogResponse?> GetLogByIdAsync(int id)
        {
            var log = await this
                ._logRepository.Query()
                .Include(l => l.User)
                .Include(l => l.Group)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (log == null)
                return null;
            return ToResponse(log);
        }

        /// <inheritdoc />
        public async Task<PagedResult<LogResponse>> GetLogsAsync(
            int page,
            int pageSize,
            string? search = null
        )
        {
            var query = this
                ._logRepository.Query()
                .Include(l => l.User)
                .Include(l => l.Group)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(l =>
                    l.IpAddress.Contains(search)
                    || (l.User != null && l.User.Name.Contains(search))
                    || (l.Group != null && l.Group.Name.Contains(search))
                );
            }

            int totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(l => l.ActionTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var responses = items.Select(ToResponse).ToList();
            return new PagedResult<LogResponse>
            {
                Items = responses,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
            };
        }

        /// <inheritdoc />
        public async Task<PagedResult<LogResponse>> GetLogsByCompetitionAsync(
            int competitionId,
            int page,
            int pageSize,
            string? search = null
        )
        {
            var query = this
                ._logRepository.Query()
                .Include(l => l.User)
                .Include(l => l.Group)
                .Where(l => l.CompetitionId == competitionId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(l =>
                    l.IpAddress.Contains(search)
                    || (l.User != null && l.User.Name.Contains(search))
                    || (l.Group != null && l.Group.Name.Contains(search))
                );
            }

            int totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(l => l.ActionTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var responses = items.Select(ToResponse).ToList();
            return new PagedResult<LogResponse>
            {
                Items = responses,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
            };
        }

        /// <inheritdoc />
        public async Task<PagedResult<LogResponse>> GetLogsByUserAsync(
            string userId,
            int page,
            int pageSize,
            string? search = null
        )
        {
            var query = this
                ._logRepository.Query()
                .Include(l => l.User)
                .Include(l => l.Group)
                .Where(l => l.UserId == userId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(l =>
                    l.IpAddress.Contains(search)
                    || (l.Group != null && l.Group.Name.Contains(search))
                );
            }

            int totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(l => l.ActionTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var responses = items.Select(ToResponse).ToList();
            return new PagedResult<LogResponse>
            {
                Items = responses,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
            };
        }

        /// <inheritdoc />
        public async Task<PagedResult<LogResponse>> GetLogsByGroupAsync(
            int groupId,
            int page,
            int pageSize,
            string? search = null
        )
        {
            var query = this
                ._logRepository.Query()
                .Include(l => l.User)
                .Include(l => l.Group)
                .Where(l => l.GroupId == groupId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(l =>
                    l.IpAddress.Contains(search) || (l.User != null && l.User.Name.Contains(search))
                );
            }

            int totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(l => l.ActionTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var responses = items.Select(ToResponse).ToList();
            return new PagedResult<LogResponse>
            {
                Items = responses,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
            };
        }

        private static LogResponse ToResponse(Log log)
        {
            return new LogResponse
            {
                Id = log.Id,
                ActionType = log.ActionType,
                ActionDescription = GetActionDescription(
                    log.ActionType,
                    log.User?.Name,
                    log.Group?.Name
                ),
                ActionTime = log.ActionTime,
                IpAddress = log.IpAddress,
                UserId = log.UserId,
                UserName = log.User?.Name,
                GroupId = log.GroupId,
                GroupName = log.Group?.Name,
                CompetitionId = log.CompetitionId,
            };
        }

        /// <summary>
        /// Gets a human-readable description for a log action type.
        /// </summary>
        /// <param name="actionType">The type of action.</param>
        /// <param name="userName">Optional user name for personalized messages.</param>
        /// <param name="groupName">Optional group name for personalized messages.</param>
        /// <returns>A human-readable description of the action.</returns>
        private static string GetActionDescription(
            LogType actionType,
            string? userName,
            string? groupName
        )
        {
            var user = userName ?? "Usuário";
            var group = groupName ?? "Grupo";

            return actionType switch
            {
                LogType.UserAction => $"{user} realizou uma ação",
                LogType.SystemAction => "Ação do sistema",
                LogType.Login => $"{user} ({group}) entrou na competição",
                LogType.Logout => $"{user} ({group}) saiu da competição",
                LogType.SubmittedExercise => $"{group} enviou uma submissão de exercício",
                LogType.GroupBlockedInCompetition => $"{group} foi bloqueado na competição",
                LogType.GroupUnblockedInCompetition => $"{group} foi desbloqueado na competição",
                LogType.QuestionSent => $"{group} enviou uma pergunta",
                LogType.AnswerGiven => $"Pergunta de {group} foi respondida",
                LogType.CompetitionUpdated => "Configurações da competição foram atualizadas",
                LogType.CompetitionFinished => "Competição foi finalizada manualmente",
                _ => "Ação desconhecida",
            };
        }
    }
}
