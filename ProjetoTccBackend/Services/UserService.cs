using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjetoTccBackend.Database.Requests.Auth;
using ProjetoTccBackend.Database.Requests.User;
using ProjetoTccBackend.Database.Responses.Global;
using ProjetoTccBackend.Database.Responses.Group;
using ProjetoTccBackend.Database.Responses.User;
using ProjetoTccBackend.Exceptions;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Repositories.Interfaces;
using ProjetoTccBackend.Services.Interfaces;

namespace ApiEstoqueASP.Services;

public class UserService : IUserService
{
    //private IMapper _mapper;
    private UserManager<User> _userManager;
    private readonly IUserRepository _userRepository;
    private readonly IGroupInviteRepository _groupInviteRepository;
    private SignInManager<User> _signInManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITokenService _tokenService;
    private readonly ILogger<UserService> _logger;

    public UserService(
        UserManager<User> userManager,
        IUserRepository userRepository,
        IGroupInviteRepository groupInviteRepository,
        SignInManager<User> signInManager,
        IHttpContextAccessor httpContextAccessor,
        ITokenService tokenService,
        ILogger<UserService> logger
    )
    {
        this._userManager = userManager;
        this._userRepository = userRepository;
        this._groupInviteRepository = groupInviteRepository;
        this._signInManager = signInManager;
        this._tokenService = tokenService;

        this._httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    /// <inheritdoc/>
    public User GetHttpContextLoggedUser()
    {
        var user = this._httpContextAccessor.HttpContext?.User;

        if (user == null)
        {
            throw new UnauthorizedAccessException("Usuário não autenticado");
        }

        var userId = user.FindFirstValue("id");

        if (userId == null)
        {
            throw new UnauthorizedAccessException("Usuário não autenticado");
        }

        var loggedUser = this
            ._userRepository.Query()
            .Include(u => u.Group)
            .ThenInclude(g => g.Users)
            .Where(u => u.Id == userId)
            .First();

        if (loggedUser == null)
        {
            throw new UnauthorizedAccessException("Usuário não autenticado");
        }

        return loggedUser;
    }

    /// <inheritdoc/>
    public async Task<Tuple<User, string>> RegisterUserAsync(RegisterUserRequest user)
    {
        User? existentUser = this._userRepository.GetByEmail(user.Email);

        this._logger.LogDebug("Test");

        if (existentUser is not null)
        {
            this._logger.LogError("Email already in use");
            throw new FormException(
                new Dictionary<string, string>() { { "email", """E-mail já utilizado""" } }
            );
        }

        if (user.Role.Equals("Admin"))
        {
            throw new FormException(
                new Dictionary<string, string>()
                {
                    { "general", "Não foi possível criar o usuário" },
                }
            );
        }

        if (user.Role.Equals("Teacher"))
        {
            string accessCode = user.AccessCode!;

            bool isValid = this._tokenService.ValidateToken(accessCode);

            if (!isValid)
            {
                throw new FormException(
                    new Dictionary<string, string>()
                    {
                        { "accessCode", "Código de acesso inválido" },
                    }
                );
            }
        }

        User newUser = new User
        {
            Name = user.Name,
            UserName = user.Email,
            Email = user.Email,
            RA = user.RA,
            JoinYear = user.JoinYear,
            EmailConfirmed = false,
            PhoneNumberConfirmed = false,
            TwoFactorEnabled = false,
        };

        IdentityResult result = await _userManager.CreateAsync(newUser, user.Password);

        if (result.Succeeded == false)
        {
            this._logger.LogDebug(result.Errors.Count().ToString());
            throw new FormException(
                new Dictionary<string, string> { { "Error", result.Errors.First().Code } }
            );
        }

        newUser.LastLoggedAt = DateTime.UtcNow;

        await this._userManager.UpdateAsync(newUser);
        //await _signInManager.UserManager.AddClaimAsync(newUser, new Claim(ClaimTypes.Role, user.Role));

        // Adiciono o usuário registrado ao role "User"
        IdentityResult res = await this._userManager.AddToRoleAsync(newUser, user.Role);

        await this._signInManager.PasswordSignInAsync(newUser, user.Password, true, false);

        if (res.Succeeded == false)
        {
            throw new FormException(
                new Dictionary<string, string> { { "Error", result.Errors.First().Code } }
            );
        }

        return Tuple.Create(newUser, user.Role);
    }

    /// <inheritdoc/>
    public async Task<Tuple<User, string>> LoginUserAsync(LoginUserRequest usr)
    {
        //Console.WriteLine($"{dto.Ra}, {dto.Password}");

        User? existentUser = this._userRepository.GetByRa(usr.Ra);

        if (existentUser == null)
        {
            throw new FormException(
                new Dictionary<string, string> { { "form", "RA e/ou senha incorreto(s)" } }
            );
        }

        SignInResult result = await this._signInManager.PasswordSignInAsync(
            existentUser,
            usr.Password,
            false,
            false
        );

        if (result.Succeeded == false)
        {
            throw new FormException(
                new Dictionary<string, string> { { "form", "RA e/ou senha incorreto(s)" } }
            );
        }

        existentUser.LastLoggedAt = DateTime.UtcNow;
        await this._userManager.UpdateAsync(existentUser);

        string userRole = (await this._userManager.GetRolesAsync(existentUser)).First();

        existentUser = await this
            ._userRepository.Query()
            .Where(u => u.Id == existentUser.Id)
            .AsSplitQuery()
            .Include(u => u.Group)
            .ThenInclude(g => g.GroupInvites)
            .ThenInclude(g => g.User)
            .Include(u => u.Group)
            .ThenInclude(g => g.Users)
            .FirstAsync();

        if (existentUser.Group != null && existentUser.Group.GroupInvites.Count == 0)
        {
            existentUser.Group.GroupInvites = await this
                ._groupInviteRepository.Query()
                .Where(g => g.GroupId == existentUser.Group.Id && g.Accepted == false)
                .Include(g => g.User)
                .ToListAsync();
        }
        

        return Tuple.Create(existentUser, userRole);
    }

    /// <inheritdoc/>
    public async Task<User?> GetUser(string id)
    {
        User? user = this._userRepository.GetById(id);

        return user;
    }

    /// <inheritdoc/>
    public async Task<List<User>> GetAllUsers()
    {
        List<User> users = this._userRepository.GetAll().ToList();

        return users;
    }

    /// <inheritdoc />
    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }

    /// <inheritdoc/>
    public async Task<PagedResult<GenericUserInfoResponse>> GetUsersAsync(
        int page,
        int pageSize,
        string? search = null,
        string? role = null
    )
    {
        var query = this._userRepository.Query();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(u =>
                u.Name.Contains(search) || u.Email.Contains(search) || u.RA.Contains(search)
            );
        }
        if (!string.IsNullOrWhiteSpace(role))
        {
            this._logger.LogDebug($"Chegou: {role}");
            var userIdsWithRole = await _userManager.GetUsersInRoleAsync(role);
            var userIds = userIdsWithRole.Select(u => u.Id).ToList();
            query = query.Where(u => userIds.Contains(u.Id));
        }
        int totalCount = query.Count();
        List<User> items = await query
            .AsSplitQuery()
            .OrderBy(e => e.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(x => x.Group)
            .ToListAsync();

        int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        return new PagedResult<GenericUserInfoResponse>
        {
            Items = items
                .Select(x => new GenericUserInfoResponse()
                {
                    Id = x.Id,
                    Ra = x.RA,
                    Name = x.Name,
                    Email = x.Email!,
                    JoinYear = x.JoinYear,
                    LastLoggedAt = x.LastLoggedAt,
                    CreatedAt = x.CreatedAt,
                    Department = x.Department,
                    Group =
                        x.Group != null
                            ? new GroupResponse()
                            {
                                Id = x.Group.Id,
                                Name = x.Group.Name,
                                LeaderId = x.Group.LeaderId,
                                Users = [],
                            }
                            : null,
                })
                .ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages,
        };
    }

    /// <inheritdoc/>
    public async Task<User?> UpdateUserAsync(string userId, UpdateUserRequest request)
    {
        var user = this._userRepository.GetById(userId);
        if (user == null)
            return null;
        user.Name = request.Name;
        user.Email = request.Email;
        user.UserName = request.Email;
        user.PhoneNumber = request.PhoneNumber;
        user.JoinYear = request.JoinYear;
        await this._userManager.UpdateAsync(user);

        return user;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteUserAsync(string userId)
    {
        var user = this._userRepository.GetById(userId);
        if (user == null)
            return false;

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
            return false;

        await this._userRepository.DeleteByIdAsync(userId);
        return true;
    }
}
