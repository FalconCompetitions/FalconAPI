using ProjetoTccBackend.Database;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Repositories.Interfaces;

namespace ProjetoTccBackend.Repositories
{
    public class GroupInCompetitionRepository : GenericRepository<GroupInCompetition>, IGroupInCompetitionRepository
    {
        public GroupInCompetitionRepository(TccDbContext dbContext) : base(dbContext)
        {
            
        }


    }
}
