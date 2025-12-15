using Microsoft.EntityFrameworkCore;
using ProjetoTccBackend.Database;
using ProjetoTccBackend.Database.Requests.Group;
using ProjetoTccBackend.Exceptions;
using ProjetoTccBackend.Exceptions.Group;
using ProjetoTccBackend.Exceptions.User;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Repositories.Interfaces;
using ProjetoTccBackend.Services.Interfaces;

namespace ProjetoTccBackend.Services
{
    /// <summary>
    /// Service responsible for managing group invitation operations.
    /// </summary>
    public class GroupInviteService : IGroupInviteService
    {
        private readonly IUserService _userService;
        private readonly IUserRepository _userRepository;
        private readonly IGroupRepository _groupRepository;
        private readonly IGroupInviteRepository _groupInviteRepository;
        private readonly ILogger<GroupInviteService> _logger;
        private readonly TccDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupInviteService"/> class.
        /// </summary>
        /// <param name="userService">The service for user operations.</param>
        /// <param name="userRepository">The repository for user data access.</param>
        /// <param name="groupRepository">The repository for group data access.</param>
        /// <param name="groupInviteRepository">The repository for group invite data access.</param>
        /// <param name="logger">Logger for registering information and errors.</param>
        /// <param name="dbContext">The database context.</param>
        public GroupInviteService(
            IUserService userService,
            IUserRepository userRepository,
            IGroupRepository groupRepository,
            IGroupInviteRepository groupInviteRepository,
            ILogger<GroupInviteService> logger,
            TccDbContext dbContext
        )
        {
            this._userService = userService;
            this._userRepository = userRepository;
            this._groupRepository = groupRepository;
            this._groupInviteRepository = groupInviteRepository;
            this._logger = logger;
            this._dbContext = dbContext;
        }

        /// <inheritdoc />
        public async Task<List<GroupInvite>> GetUserGroupInvites(string userId)
        {
            List<GroupInvite> groupInvitations = await this
                ._groupInviteRepository.Query()
                .Include(g => g.Group)
                .ThenInclude(g => g.Users)
                .Where(g => g.UserId == userId && g.Accepted == false)
                .ToListAsync();

            return groupInvitations;
        }

        /// <inheritdoc />
        public async Task<GroupInvite?> SendGroupInviteToUser(InviteUserToGroupRequest request)
        {
            const int MAX_GROUP_MEMBERS = 3;

            User loggedUser = this._userService.GetHttpContextLoggedUser();

            User? user = await this
                ._userRepository.Query()
                .Where(u => u.RA == request.RA)
                .Include(u => u.Group)
                .FirstOrDefaultAsync();
            Group? group = await this
                ._groupRepository.Query()
                .Include(g => g.Users)
                .Include(g => g.GroupInvites.Where(inv => !inv.Accepted))
                .Where(g => g.Id == request.GroupId)
                .FirstOrDefaultAsync();

            if (user is null)
            {
                throw new UserNotFoundException(request.RA);
            }

            if (group is null)
            {
                return null;
            }

            if (user.Group is not null)
            {
                throw new UserHasGroupException();
            }

            if (loggedUser.Id != group.LeaderId)
            {
                throw new UserNotGroupLeaderException();
            }

            int currentMembersCount = group.Users.Count;
            int pendingInvitesCount = group.GroupInvites.Count(inv => !inv.Accepted);

            if (currentMembersCount + pendingInvitesCount >= MAX_GROUP_MEMBERS)
            {
                throw new MaxMembersExceededException($"O grupo já possui {currentMembersCount} membros e {pendingInvitesCount} convites pendentes. Limite máximo: {MAX_GROUP_MEMBERS}");
            }

            GroupInvite? existentInvitation = await this
                ._groupInviteRepository.Query()
                .Where(g => g.GroupId == group.Id && g.UserId == user.Id && g.Accepted == false)
                .FirstOrDefaultAsync();

            if (existentInvitation is not null)
            {
                return null;
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
                .Where(g =>
                    g.GroupId == groupId && g.UserId == loggedUser.Id && g.Accepted == false
                )
                .FirstOrDefaultAsync();

            if (invite is null)
            {
                throw new GroupInvitationException();
            }

            if (invite.UserId != loggedUser.Id)
            {
                return null;
            }

            invite.Accepted = true;
            loggedUser.GroupId = groupId;
            this._groupInviteRepository.Update(invite);
            this._userRepository.Update(loggedUser);

            await this._dbContext.SaveChangesAsync();

            invite = await this
                ._groupInviteRepository.Query()
                .Where(x => x.GroupId == loggedUser.GroupId && x.UserId == loggedUser.Id)
                .Include(g => g.Group)
                .ThenInclude(g => g.Users)
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

        /// <inheritdoc />
        public async Task<bool?> RemoveUserFromGroupAsync(int groupId, string userId)
        {
            User loggedUser = this._userService.GetHttpContextLoggedUser();

            if (groupId != loggedUser.GroupId)
            {
                return null;
            }

            if (loggedUser.Group!.Users.Select(u => u.Id).Contains(userId) == false)
            {
                return false;
            }

            if (loggedUser.Id != userId && loggedUser.Group.LeaderId != loggedUser.Id)
            {
                return false;
            }

            User? selectedUser = await this
                ._userRepository.Query()
                .Where(u => u.Id == userId)
                .FirstOrDefaultAsync();

            if (selectedUser == null)
            {
                return null;
            }

            // If the user being removed is the leader
            if (selectedUser.Id == loggedUser.Group.LeaderId)
            {
                Group group = await this
                    ._groupRepository.Query()
                    .Include(g => g.Users)
                    .Where(g => g.Id == groupId)
                    .FirstAsync();

                var otherUsers = group.Users.Where(u => u.Id != selectedUser.Id).OrderBy(u => u.Name).ToList();

                if (otherUsers.Count == 0)
                {
                    // No other users, delete group
                    selectedUser.GroupId = null;
                    this._userRepository.Update(selectedUser);
                    await this._dbContext.SaveChangesAsync();

                    await this
                        ._groupInviteRepository.Query()
                        .Where(g => g.GroupId == groupId)
                        .ExecuteDeleteAsync();

                    this._groupRepository.Remove(group);
                    await this._dbContext.SaveChangesAsync();

                    return true;
                }
                else
                {
                    // Transfer leadership to next user
                    User nextGroupLeader = otherUsers.First();
                    group.LeaderId = nextGroupLeader.Id;

                    // Remove leader from group
                    selectedUser.GroupId = null;
                    this._userRepository.Update(selectedUser);
                    this._groupRepository.Update(group);

                    await this
                        ._groupInviteRepository.Query()
                        .Where(g => g.UserId == selectedUser.Id && g.GroupId == groupId)
                        .ExecuteDeleteAsync();

                    await this._dbContext.SaveChangesAsync();

                    return true;
                }
            }

            // If not leader, just remove from group
            selectedUser.GroupId = null;
            this._userRepository.Update(selectedUser);
            await this._dbContext.SaveChangesAsync();

            return true;
        }
    }
}
