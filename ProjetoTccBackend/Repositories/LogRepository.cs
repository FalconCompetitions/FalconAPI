using ProjetoTccBackend.Database;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace ProjetoTccBackend.Repositories
{
    public class LogRepository : GenericRepository<Log>, ILogRepository
    {
        public LogRepository(TccDbContext dbContext) : base(dbContext)
        {
        }

        public IEnumerable<Log> GetByCompetitionId(int competitionId)
        {
            return this._dbContext.Logs.Where(l => l.CompetitionId == competitionId).ToList();
        }

        public IEnumerable<Log> GetByUserId(string userId)
        {
            return this._dbContext.Logs.Where(l => l.UserId == userId).ToList();
        }

        public IEnumerable<Log> GetByGroupId(int groupId)
        {
            return this._dbContext.Logs.Where(l => l.GroupId == groupId).ToList();
        }
    }
}
