using Microsoft.EntityFrameworkCore;
using ProjetoTccBackend.Database;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Repositories.Interfaces;

namespace ProjetoTccBackend.Repositories
{
    public class GroupInviteRepository : GenericRepository<GroupInvite>, IGroupInviteRepository
    {
        public GroupInviteRepository(TccDbContext dbContext)
            : base(dbContext) { }


        /// <inheritdoc />
        public async Task<Group?> IsUserInGroupById(string userId)
        {
            Group? group = await this.Query()
                .Where(g => g.UserId.Equals(userId))
                .Include(g => g.Group)
                .Select(g => g.Group)
                .FirstOrDefaultAsync();

            return group;
        }


        /// <inheritdoc />
        public async Task<ICollection<User>> GetUsersInGroupById(int groupId)
        {
            List<User> usersInGroup = await this.Query()
                .Where(g => g.GroupId.Equals(groupId))
                .Include(g => g.User)
                .Select(g => g.User)
                .ToListAsync();

            return usersInGroup;
        }
    }
}
