using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using ProjetoTccBackend.Database;
using ProjetoTccBackend.Database.Requests.Group;
using ProjetoTccBackend.Database.Responses.Global;
using ProjetoTccBackend.Database.Responses.Group;
using ProjetoTccBackend.Database.Responses.User;
using ProjetoTccBackend.Exceptions;
using ProjetoTccBackend.Exceptions.Group;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Repositories.Interfaces;
using ProjetoTccBackend.Services.Interfaces;

namespace ProjetoTccBackend.Services
{
    /// <inheritdoc />
    public class GroupService : IGroupService
    {
        private readonly IUserService _userService;
        private readonly IUserRepository _userRepository;
        private readonly IGroupRepository _groupRepository;
        private readonly IGroupInviteService _groupInviteService;
        private readonly ILogger<GroupService> _logger;
        private readonly TccDbContext _dbContext;
        private const int MAX_MEMBERS_PER_GROUP = 3;

        public GroupService(
            IUserService userService,
            IUserRepository userRepository,
            IGroupRepository groupRepository,
            IGroupInviteService groupInviteService,
            TccDbContext dbContext,
            ILogger<GroupService> logger
        )
        {
            this._userService = userService;
            this._userRepository = userRepository;
            this._groupRepository = groupRepository;
            this._groupInviteService = groupInviteService;
            this._dbContext = dbContext;
            this._logger = logger;
        }

        /// <inheritdoc/>
        public async Task<Group> CreateGroupAsync(CreateGroupRequest groupRequest)
        {
            User loggedUser = this._userService.GetHttpContextLoggedUser();

            Group? existentGroup = await this
                ._groupRepository.Query()
                .Where(g => g.LeaderId == loggedUser.Id)
                .FirstOrDefaultAsync();

            if (existentGroup != null)
            {
                throw new UserHasGroupException();
            }

            Group newGroup = new Group { Name = groupRequest.Name, LeaderId = loggedUser.Id };

            this._groupRepository.Add(newGroup);

            if (loggedUser == null)
            {
                throw new UnauthorizedAccessException("Usuário não autenticado");
            }

            await this._dbContext.SaveChangesAsync();

            loggedUser.GroupId = newGroup.Id;

            this._userRepository.Update(loggedUser);

            await this._dbContext.SaveChangesAsync();

            if (groupRequest.UserRAs != null)
            {
                foreach (string ra in groupRequest.UserRAs)
                {
                    User? user = await this._userRepository.GetByIdAsync(ra);

                    if (user == null)
                    {
                        continue;
                    }

                    await this._groupInviteService.SendGroupInviteToUser(
                        new InviteUserToGroupRequest() { GroupId = newGroup.Id, RA = user.RA }
                    );
                }
            }

            newGroup = await this
                ._groupRepository.Query()
                .Include(g => g.Users)
                .Include(g => g.GroupInvites)
                .ThenInclude(g => g.User)
                .Where(g => g.Id == newGroup.Id)
                .FirstAsync();

            return newGroup;
        }

        /// <inheritdoc/>
        public Group? ChangeGroupName(ChangeGroupNameRequest groupRequest)
        {
            User loggedUser = this._userService.GetHttpContextLoggedUser();
            Group? group = this._groupRepository.GetById(groupRequest.Id);

            if (group == null)
            {
                return null;
            }

            if (loggedUser.GroupId != group.Id)
            {
                throw new UnauthorizedAccessException(
                    "Usuário não pode mudar o nome do grupo requisitado"
                );
            }

            group.Name = groupRequest.Name;
            this._groupRepository.Update(group);

            this._dbContext.SaveChanges();

            return group;
        }

        /// <inheritdoc/>
        public Group? GetGroupById(int id)
        {
            User loggedUser = this._userService.GetHttpContextLoggedUser();
            
            if (loggedUser.GroupId != id)
            {
                throw new UnauthorizedAccessException("Não possui acesso ao grupo requisitado");
            }

            Group? group = this
                ._groupRepository.Query()
                .Include(g => g.Users)
                .Where(g => g.Id == id)
                .FirstOrDefault();

            return group;
        }

        /// <inheritdoc/>
        public async Task<PagedResult<GroupResponse>> GetGroupsAsync(
            int page,
            int pageSize,
            string? search
        )
        {
            var query = this._groupRepository.Query();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(g => g.Name.Contains(search));
            }

            int totalCount = query.Count();
            List<Group> items = await query
                .AsSplitQuery()
                .OrderBy(e => e.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(x => x.Users)
                .ToListAsync();

            List<GroupResponse> groupResponses = new List<GroupResponse>();

            foreach (var item in items)
            {
                List<GenericUserInfoResponse> userInfoResponses =
                    new List<GenericUserInfoResponse>();

                foreach (var user in item.Users)
                {
                    userInfoResponses.Add(
                        new GenericUserInfoResponse()
                        {
                            Id = user.Id,
                            Ra = user.RA,
                            Name = user.UserName!,
                            Email = user.Email!,
                            JoinYear = (int)user.JoinYear!,
                            CreatedAt = user.CreatedAt,
                            LastLoggedAt = user.LastLoggedAt,
                            Department = user.Department,
                        }
                    );
                }

                groupResponses.Add(
                    new GroupResponse()
                    {
                        Id = item.Id,
                        Name = item.Name,
                        LeaderId = item.LeaderId,
                        Users = userInfoResponses,
                    }
                );
            }

            return new PagedResult<GroupResponse>
            {
                Items = groupResponses,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
            };
        }

        /// <inheritdoc/>
        public async Task<GroupResponse?> UpdateGroupAsync(
            int groupId,
            UpdateGroupRequest request,
            string userId,
            IList<string> userRoles
        )
        {
            var group = await this
                ._groupRepository.Query()
                .Include(g => g.Users)
                .Where(g => g.Id == groupId && g.LeaderId == userId)
                .FirstOrDefaultAsync();

            if (group == null)
                return null;
            bool isAdmin = userRoles.Contains("Admin");
            bool isLeader = group.LeaderId == userId;
            if (!isAdmin && !isLeader)
                return null;

            group.Name = request.Name;
            // Atualiza os usuários do grupo

            this._groupRepository.Update(group);
            this._dbContext.SaveChanges();

            group = await this
                ._groupRepository.Query()
                .Include(g => g.Users)
                .Where(g => g.Id == groupId)
                .FirstAsync();

            GroupResponse response = new GroupResponse()
            {
                Id = group.Id,
                LeaderId = group.LeaderId,
                Name = group.Name,
                Users = group
                    .Users.Select(user => new GenericUserInfoResponse()
                    {
                        Id = user.Id,
                        Email = user.Email!,
                        JoinYear = (int)user.JoinYear!,
                        Name = user.Name,
                        CreatedAt = user.CreatedAt,
                    })
                    .ToList(),
            };

            return response;
        }
    }
}
