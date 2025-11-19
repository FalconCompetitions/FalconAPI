using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ProjetoTccBackend.Database;
using ProjetoTccBackend.Database.Requests.Group;
using ProjetoTccBackend.Database.Responses.Competition;
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
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const int MAX_MEMBERS_PER_GROUP = 3;

        public GroupService(
            IUserService userService,
            IUserRepository userRepository,
            IGroupRepository groupRepository,
            IGroupInviteService groupInviteService,
            TccDbContext dbContext,
            ILogger<GroupService> logger,
            IHttpContextAccessor httpContextAccessor
        )
        {
            this._userService = userService;
            this._userRepository = userRepository;
            this._groupRepository = groupRepository;
            this._groupInviteService = groupInviteService;
            this._dbContext = dbContext;
            this._logger = logger;
            this._httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Verifica se o usuário logado é Admin ou Teacher
        /// </summary>
        private bool IsAdminOrTeacher()
        {
            var userRoles = this._httpContextAccessor.HttpContext?.User
                .Claims.Where(c => c.Type.Equals(ClaimTypes.Role))
                .Select(c => c.Value)
                .ToList();

            return userRoles != null && (userRoles.Contains("Admin") || userRoles.Contains("Teacher"));
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

            await this._dbContext.SaveChangesAsync();

            loggedUser.GroupId = newGroup.Id;

            this._userRepository.Update(loggedUser);

            await this._dbContext.SaveChangesAsync();

            if (groupRequest.UserRAs != null)
            {
                foreach (string ra in groupRequest.UserRAs)
                {
                    User? user = this._userRepository.GetByRa(ra);

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

            bool isAdminOrTeacher = this.IsAdminOrTeacher();
            bool isLeader = group.LeaderId == loggedUser.Id;

            // Admin, Teacher ou líder do grupo podem alterar o nome
            if (!isAdminOrTeacher && !isLeader)
            {
                throw new FormException(
                    new Dictionary<string, string> { { "form", "Você não tem permissão para alterar o nome deste grupo" } }
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
            bool isAdminOrTeacher = this.IsAdminOrTeacher();

            // Admin e Teacher podem acessar qualquer grupo, Student só pode acessar o próprio
            if (!isAdminOrTeacher && loggedUser.GroupId != id)
            {
                throw new FormException(
                    new Dictionary<string, string> { { "form", "Você não tem permissão para acessar este grupo" } }
                );
            }

            Group? group = this
                ._groupRepository.Query()
                .Include(g => g.Users)
                .Include(g => g.GroupInvites)
                .ThenInclude(g => g.User)
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
                            JoinYear = user.JoinYear,
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
            bool isAdmin = userRoles.Contains("Admin");
            bool isTeacher = userRoles.Contains("Teacher");

            var group = await this
                ._groupRepository.Query()
                .Include(g => g.Users)
                .Where(g => g.Id == groupId)
                .FirstOrDefaultAsync();

            if (group == null)
                return null;

            bool isLeader = group.LeaderId == userId;
            if (!isAdmin && !isTeacher && !isLeader)
                return null;

            group.Name = request.Name;

            foreach (string id in request.MembersToRemove)
            {
                bool? res = await this._groupInviteService.RemoveUserFromGroupAsync(groupId, id);
            }

            this._groupRepository.Update(group);
            try
            {
                this._dbContext.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new GroupConcurrencySuccessException();
            }

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
                        JoinYear = user.JoinYear,
                        Name = user.Name,
                        CreatedAt = user.CreatedAt,
                    })
                    .ToList(),
            };

            return response;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteGroupAsync(int groupId, string userId, IList<string> userRoles)
        {
            bool isAdmin = userRoles.Contains("Admin");
            bool isTeacher = userRoles.Contains("Teacher");

            var group = await this
                ._groupRepository.Query()
                .Include(g => g.Users)
                .Include(g => g.GroupInvites)
                .Where(g => g.Id == groupId)
                .FirstOrDefaultAsync();

            if (group == null)
                return false;

            bool isLeader = group.LeaderId == userId;

            // Somente Admin, Teacher ou líder do grupo podem deletar
            if (!isAdmin && !isTeacher && !isLeader)
                return false;

            // Remove todas as associações de usuários com o grupo
            foreach (var user in group.Users)
            {
                user.GroupId = null;
                this._userRepository.Update(user);
            }

            // Remove todos os convites pendentes do grupo
            if (group.GroupInvites.Any())
            {
                this._dbContext.GroupInvites.RemoveRange(group.GroupInvites);
            }

            // Remove o grupo
            this._groupRepository.Remove(group);

            try
            {
                await this._dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Erro ao deletar grupo {GroupId}", groupId);
                return false;
            }
        }
    }
}
