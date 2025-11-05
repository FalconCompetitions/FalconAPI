using Microsoft.EntityFrameworkCore;
using ProjetoTccBackend.Database;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Repositories.Interfaces;

namespace ProjetoTccBackend.Repositories
{
    public class GroupRepository : GenericRepository<Group>, IGroupRepository
    {
        public GroupRepository(TccDbContext dbContext) : base(dbContext)
        {
        }

        /// <summary>
        /// Gets a group by ID with its users included.
        /// </summary>
        /// <param name="id">The group ID.</param>
        /// <returns>The group with users, or null if not found.</returns>
        public Group? GetByIdWithUsers(int id)
        {
            return this._dbContext.Groups
                .Include(g => g.Users)
                .FirstOrDefault(g => g.Id == id);
        }
    }
}
