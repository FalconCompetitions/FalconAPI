using ProjetoTccBackend.Models;
using System.Collections.Generic;

namespace ProjetoTccBackend.Repositories.Interfaces
{
    public interface ILogRepository : IGenericRepository<Log>
    {
        IEnumerable<Log> GetByCompetitionId(int competitionId);
        IEnumerable<Log> GetByUserId(string userId);
        IEnumerable<Log> GetByGroupId(int groupId);
    }
}
