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
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Repositories.Interfaces;
using ProjetoTccBackend.Services.Interfaces;

namespace ProjetoTccBackend.Services
{
    public class GroupService : IGroupService
    {
        private readonly IUserService _userService;
        private readonly IUserRepository _userRepository;
        private readonly IGroupRepository _groupRepository;
        private readonly ILogger<GroupService> _logger;
        private readonly TccDbContext _dbContext;

        public GroupService(
            IUserService userService,
            IUserRepository userRepository,
            IGroupRepository groupRepository,
            TccDbContext dbContext,
            ILogger<GroupService> logger
        )
        {
            this._userService = userService;
            this._userRepository = userRepository;
            this._groupRepository = groupRepository;
            this._dbContext = dbContext;
            this._logger = logger;
        }

        /// <inheritdoc/>
        public async Task<Group> CreateGroupAsync(CreateGroupRequest groupRequest)
        {
            User loggedUser = this._userService.GetHttpContextLoggedUser();

            Group newGroup = new Group { Name = groupRequest.Name, LeaderId = loggedUser.Id };

            this._groupRepository.Add(newGroup);

            if (loggedUser == null)
            {
                throw new UnauthorizedAccessException("Usuário não autenticado");
            }

            loggedUser.GroupId = newGroup.Id;
            this._userRepository.Update(loggedUser);

            await this._dbContext.SaveChangesAsync();

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

            Group? group = this._groupRepository.GetById(id);

            return group;
        }

        /// <inheritdoc/>
        public async Task<PagedResult<GroupResponse>> GetGroupsAsync(
            int page,
            int pageSize,
            string? search
        )
        {
            var query = this._groupRepository.GetAll().AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(g => g.Name.Contains(search));
            }

            query = query.Include(g => g.Users);

            int totalCount = query.Count();
            var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

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
                            Name = user.UserName!,
                            Email = user.Email!,
                            JoinYear = (int)user.JoinYear!,
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

            return await Task.FromResult(
                new PagedResult<GroupResponse>
                {
                    Items = groupResponses,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                }
            );
        }

        /// <inheritdoc/>
        public async Task<GroupResponse?> UpdateGroupAsync(
            int groupId,
            UpdateGroupRequest request,
            string userId,
            IList<string> userRoles
        )
        {
            var group = this._groupRepository.GetById(groupId);
            if (group == null)
                return null;
            // Permissão: Admin, Teacher ou membro do grupo
            bool isAdmin = userRoles.Contains("Admin");
            bool isTeacher = userRoles.Contains("Teacher");
            bool isMember = group.Users.Any(u => u.Id == userId);
            if (!(isAdmin || isTeacher || isMember))
                return null;
            group.Name = request.Name;
            // Atualiza os usuários do grupo
            List<User> currentUsers = group.Users.ToList();
            var newUsers = this
                ._userRepository.GetAll()
                .Where(u => request.UserIds.Contains(u.Id))
                .ToList();

            var users = currentUsers.ToList();

            // Remove usuários que não estão mais
            foreach (var user in currentUsers)
            {
                if (!request.UserIds.Contains(user.Id))
                {
                    user.GroupId = null;
                    this._userRepository.Update(user);

                    int indexToRemove = users.IndexOf(user);
                    users.RemoveAt(indexToRemove);
                }
            }
            // Adiciona novos usuários
            foreach (var user in newUsers)
            {
                if (user.GroupId != group.Id)
                {
                    user.GroupId = group.Id;
                    this._userRepository.Update(user);
                    users.Add(user);
                }
            }
            this._groupRepository.Update(group);
            this._dbContext.SaveChanges();

            GroupResponse response = new GroupResponse()
            {
                Id = group.Id,
                LeaderId = group.LeaderId,
                Name = group.Name,
                Users = users.Select(user => new GenericUserInfoResponse()
                {
                    Id = user.Id,
                    Email = user.Email!,
                    JoinYear = (int)user.JoinYear!,
                    Name = user.Name
                }).ToList(),
            };

            return response;
        }
    }
}
