using ProjetoTccBackend.Database;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Repositories.Interfaces;

namespace ProjetoTccBackend.Repositories
{
    public class AttachedFileRepository : GenericRepository<AttachedFile>, IAttachedFileRepository
    {
        public AttachedFileRepository(TccDbContext dbContext) : base(dbContext)
        {

        }
    }
}
