using ProjetoTccBackend.Models;
using ProjetoTccBackend.Services.Interfaces;
using ProjetoTccBackend.Exceptions;
//using AutoMapper;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using ProjetoTccBackend.Repositories.Interfaces;
using ProjetoTccBackend.Database.Requests.Auth;

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
    public User GetHttpContextLoggerUser()
    {
        var user = this._httpContextAccessor.HttpContext?.User;

        if (user == null)
        {
            throw new UnauthorizedAccessException("Usuário não autenticado");
        }

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

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
            UserName = user.UserName,
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
        //Console.WriteLine($"{dto.Email}, {dto.Password}");

        User? existentUser = this._userRepository.GetByEmail(usr.Email);

        if (existentUser == null)
        {
            throw new FormException(new Dictionary<string, string>
            {
                { "form", "Email e/ou senha incorreto(s)" }
            });
        }

        SignInResult result = await this._signInManager.PasswordSignInAsync(existentUser, usr.Password, false, false);

        if (result.Succeeded == false)
        {
            throw new FormException(new Dictionary<string, string>
            {
                { "form", "Email e/ou senha incorreto(s)" }
            });
        }

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
}
