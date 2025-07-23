using ProjetoTccBackend.Database;
using ProjetoTccBackend.Database.Requests.Group;
using ProjetoTccBackend.Exceptions;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Repositories.Interfaces;
using ProjetoTccBackend.Services.Interfaces;
using System.Security.Claims;

namespace ProjetoTccBackend.Services
{
    public class GroupService : IGroupService
    {

        private readonly IUserService _userService;
        private readonly IUserRepository _userRepository;
        private readonly IGroupRepository _groupRepository;
        private readonly ILogger<GroupService> _logger;
        private readonly TccDbContext _dbContext;

        public GroupService(IUserService userService, IUserRepository userRepository, IGroupRepository groupRepository, TccDbContext dbContext, ILogger<GroupService> logger)
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
            User loggedUser = this._userService.GetHttpContextLoggerUser();

            Group newGroup = new Group
            {
                Name = groupRequest.Name,
            };

            this._groupRepository.Add(newGroup);

            if (loggedUser == null)
            {
                throw new UnauthorizedAccessException("Usuário não autenticado");
            }

            loggedUser.GroupId = newGroup.Id;
            this._userRepository.Update(loggedUser);

            this._dbContext.SaveChanges();

            return await Task.FromResult(newGroup);
        }

        /// <inheritdoc/>
        public Group? ChangeGroupName(ChangeGroupNameRequest groupRequest)
        {
            User loggedUser = this._userService.GetHttpContextLoggerUser();
            Group? group = this._groupRepository.GetById(groupRequest.Id);

            if (group == null)
            {
                return null;
            }

            if (loggedUser.GroupId != group.Id)
            {
                throw new UnauthorizedAccessException("Usuário não pode mudar o nome do grupo requisitado");
            }

            group.Name = groupRequest.Name;
            this._groupRepository.Update(group);

            this._dbContext.SaveChanges();

            return group;
        }

        /// <inheritdoc/>
        public Group? GetGroupById(int id)
        {
            User loggedUser = this._userService.GetHttpContextLoggerUser();

            if (loggedUser.GroupId != id)
            {
                throw new UnauthorizedAccessException("Não possui acesso ao grupo requisitado");
            }

            Group? group = this._groupRepository.GetById(id);

            return group;
        }
    }
}
