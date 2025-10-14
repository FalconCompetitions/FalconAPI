using Microsoft.EntityFrameworkCore;
using ProjetoTccBackend.Database;
using ProjetoTccBackend.Database.Requests.Group;
using ProjetoTccBackend.Exceptions.Group;
using ProjetoTccBackend.Exceptions.User;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Repositories.Interfaces;
using ProjetoTccBackend.Services.Interfaces;

namespace ProjetoTccBackend.Services
{
    public class GroupInviteService : IGroupInviteService
    {
        private readonly IUserService _userService;
        private readonly IUserRepository _userRepository;
        private readonly IGroupRepository _groupRepository;
        private readonly IGroupInviteRepository _groupInviteRepository;
        private readonly TccDbContext _dbContext;

        public GroupInviteService(
            IUserService userService,
            IUserRepository userRepository,
            IGroupRepository groupRepository,
            IGroupInviteRepository groupInviteRepository,
            TccDbContext dbContext
        )
        {
            this._userService = userService;
            this._userRepository = userRepository;
            this._groupRepository = groupRepository;
            this._groupInviteRepository = groupInviteRepository;
            this._dbContext = dbContext;
        }

        /// <inheritdoc />
        public async Task<List<GroupInvite>> GetUserGroupInvites(string userId)
        {
            List<GroupInvite> groupInvitations = await this
                ._groupInviteRepository.Query()
                .Where(g => g.UserId == userId && g.Accepted == false)
                .ToListAsync();

            return groupInvitations;
        }

        /// <inheritdoc />
        public async Task<GroupInvite?> SendGroupInviteToUser(InviteUserToGroupRequest request)
        {
            User? user = await this
                ._userRepository.Query()
                .Where(u => u.Id.Equals(request.UserId))
                .Include(u => u.Group)
                .FirstOrDefaultAsync();
            Group? group = await this._groupRepository.GetByIdAsync(request.GroupId);

            if (user is null)
            {
                throw new UserNotFoundException(request.UserId);
            }

            if (group is null)
            {
                return null;
            }

            if (user.Group is not null)
            {
                throw new UserHasGroupException();
            }

            GroupInvite? invite = new GroupInvite()
            {
                GroupId = group.Id,
                UserId = user.Id,
                Accepted = false,
            };

            this._groupInviteRepository.Add(invite);
            await this._dbContext.SaveChangesAsync();

            invite = await this
                ._groupInviteRepository.Query()
                .Where(g => g.Id.Equals(invite.Id))
                .Include(g => g.Group)
                .Include(g => g.User)
                .FirstAsync();

            return invite;
        }

        /// <inheritdoc />
        public async Task<GroupInvite?> AcceptGroupInviteAsync(int groupId)
        {
            User loggedUser = this._userService.GetHttpContextLoggedUser();

            GroupInvite? invite = await this
                ._groupInviteRepository.Query()
                .Where(g => g.GroupId.Equals(groupId))
                .FirstOrDefaultAsync();

            if (invite is null)
            {
                throw new GroupInvitationException();
            }

            if (!invite.UserId.Equals(loggedUser.Id))
            {
                return null;
            }

            invite.Accepted = true;
            this._groupInviteRepository.Update(invite);

            await this._dbContext.SaveChangesAsync();

            invite = await this
                ._groupInviteRepository.Query()
                .Where(x => x.Id.Equals(invite.Id))
                .Include(g => g.Group)
                .FirstAsync();

            List<GroupInvite> remainingInvites = await this
                ._groupInviteRepository.Query()
                .Where(g => g.Id != invite.Id && g.UserId == loggedUser.Id)
                .ToListAsync();

            if (remainingInvites.Count > 0)
            {
                this._groupInviteRepository.RemoveRange(remainingInvites);
                await this._dbContext.SaveChangesAsync();
            }

            return invite;
        }
    }
}
