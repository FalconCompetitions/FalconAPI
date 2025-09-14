using ProjetoTccBackend.Models;
using ProjetoTccBackend.Services.Interfaces;
using ProjetoTccBackend.Exceptions;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using ProjetoTccBackend.Repositories.Interfaces;
using ProjetoTccBackend.Database.Requests.Auth;
using ProjetoTccBackend.Database.Responses.Global;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using ProjetoTccBackend.Database.Requests.User;

namespace ApiEstoqueASP.Services;

public class UserService : IUserService
{
    //private IMapper _mapper;
    private UserManager<User> _userManager;
    private readonly IUserRepository _userRepository;
    private SignInManager<User> _signInManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITokenService _tokenService;
    private readonly ILogger<UserService> _logger;

    public UserService(UserManager<User> userManager, IUserRepository userRepository, SignInManager<User> signInManager, IHttpContextAccessor httpContextAccessor, ITokenService tokenService, ILogger<UserService> logger)
    {
        this._userManager = userManager;
        this._userRepository = userRepository;
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

        var loggedUser = this._userRepository.GetById(userId);

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
            throw new FormException(new Dictionary<string, string>()
                {
                    { "email", """E-mail já utilizado""" }
                });
        }

        if(user.Role.Equals("Admin"))
        {
            throw new FormException(new Dictionary<string, string>()
            {
                { "general", "Não foi possível criar o usuário" }
            });
        }

        if(user.Role.Equals("Teacher"))
        {
            string accessCode = user.AccessCode!;

            bool isValid = this._tokenService.ValidateToken(accessCode);

            if(!isValid)
            {
                throw new FormException(new Dictionary<string, string>()
                {
                    { "accessCode", "Código de acesso inválido" }
                });
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
            throw new FormException(new Dictionary<string, string>
                {
                    { "Error", result.Errors.First().Code }
                });
        }

        newUser.LastLoggedAt = DateTime.UtcNow;

        await this._userManager.UpdateAsync(newUser);
        //await _signInManager.UserManager.AddClaimAsync(newUser, new Claim(ClaimTypes.Role, user.Role));

        // Adiciono o usuário registrado ao role "User"
        IdentityResult res = await this._userManager.AddToRoleAsync(newUser, user.Role);

        if (res.Succeeded == false)
        {
            throw new FormException(new Dictionary<string, string>
                {
                    { "Error", result.Errors.First().Code }
                });
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
            throw new FormException(new Dictionary<string, string>
            {
                { "form", "RA e/ou senha incorreto(s)" }
            });
        }

        SignInResult result = await this._signInManager.PasswordSignInAsync(existentUser, usr.Password, false, false);

        if (result.Succeeded == false)
        {
            throw new FormException(new Dictionary<string, string>
            {
                { "form", "RA e/ou senha incorreto(s)" }
            });
        }

        existentUser.LastLoggedAt = DateTime.UtcNow;
        await this._userManager.UpdateAsync(existentUser);

        string userRole = (await this._userManager.GetRolesAsync(existentUser)).First();

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
    public async Task<PagedResult<User>> GetUsersAsync(int page, int pageSize, string? search = null, string? role = null)
    {
        var query = this._userRepository.GetAll().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(u => u.UserName.Contains(search) || u.Email.Contains(search));
        }
        if (!string.IsNullOrWhiteSpace(role))
        {
            var userIdsWithRole = await _userManager.GetUsersInRoleAsync(role);
            var userIds = userIdsWithRole.Select(u => u.Id).ToList();
            query = query.Where(u => userIds.Contains(u.Id));
        }
        int totalCount = query.Count();
        var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        return new PagedResult<User>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages
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
}
