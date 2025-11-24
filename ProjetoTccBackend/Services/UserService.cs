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
using ProjetoTccBackend.Enums.Competition;

namespace ApiEstoqueASP.Services;

/// <summary>
/// Service responsible for user management operations.
/// </summary>
public class UserService : IUserService
{
    //private IMapper _mapper;
    private UserManager<User> _userManager;
    private readonly IUserRepository _userRepository;
    private readonly IGroupInviteRepository _groupInviteRepository;
    private readonly ICompetitionRankingRepository _competitionRankingRepository;
    private SignInManager<User> _signInManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITokenService _tokenService;
    private readonly ILogger<UserService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserService"/> class.
    /// </summary>
    /// <param name="userManager">Manager for user operations.</param>
    /// <param name="userRepository">Repository for user data access.</param>
    /// <param name="groupInviteRepository">Repository for group invite data access.</param>
    /// <param name="competitionRankingRepository">Repository for competition ranking data access.</param>
    /// <param name="signInManager">Manager for sign-in operations.</param>
    /// <param name="httpContextAccessor">Accessor for HTTP context.</param>
    /// <param name="tokenService">Service for token operations.</param>
    /// <param name="logger">Logger for registering information and errors.</param>
    public UserService(
        UserManager<User> userManager,
        IUserRepository userRepository,
        IGroupInviteRepository groupInviteRepository,
        ICompetitionRankingRepository competitionRankingRepository,
        SignInManager<User> signInManager,
        IHttpContextAccessor httpContextAccessor,
        ITokenService tokenService,
        ILogger<UserService> logger
    )
    {
        this._userManager = userManager;
        this._userRepository = userRepository;
        this._groupInviteRepository = groupInviteRepository;
        this._competitionRankingRepository = competitionRankingRepository;
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

        if (existentUser is not null)
        {
            this._logger.LogWarning("Registration attempt with existing email: {Email}", user.Email);
            throw new FormException(
                new Dictionary<string, string>() { { "email", "E-mail já utilizado" } }
            );
        }

        // Validar se o RA já existe
        User? existentUserByRa = this._userRepository.GetByRa(user.RA);
        if (existentUserByRa is not null)
        {
            this._logger.LogWarning("Registration attempt with existing RA: {RA}", user.RA);
            throw new FormException(
                new Dictionary<string, string>() { { "ra", "RA já cadastrado no sistema" } }
            );
        }

        if (user.Role.Equals("Admin"))
        {
            this._logger.LogWarning("Attempt to register Admin user via public endpoint");
            throw new FormException(
                new Dictionary<string, string>()
                {
                    { "form", "Não foi possível criar o usuário" },
                }
            );
        }

        if (user.Role.Equals("Teacher"))
        {
            string accessCode = user.AccessCode!;

            bool isValid = this._tokenService.ValidateToken(accessCode);

            if (!isValid)
            {
                this._logger.LogWarning("Invalid teacher access code provided");
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
            this._logger.LogWarning("User registration failed: {Errors}", string.Join(", ", result.Errors.Select(e => e.Code)));

            // Mapear erros do Identity para mensagens amigáveis em PT-BR
            var errorMessages = new Dictionary<string, string>();

            foreach (var error in result.Errors)
            {
                string message = error.Code switch
                {
                    "PasswordTooShort" => "A senha deve ter no mínimo 8 caracteres",
                    "PasswordRequiresDigit" => "A senha deve conter pelo menos um número",
                    "PasswordRequiresLower" => "A senha deve conter letras minúsculas",
                    "PasswordRequiresUpper" => "A senha deve conter letras maiúsculas",
                    "PasswordRequiresNonAlphanumeric" => "A senha deve conter caracteres especiais",
                    "DuplicateUserName" => "Este e-mail já está em uso",
                    "DuplicateEmail" => "Este e-mail já está em uso",
                    "InvalidEmail" => "Formato de e-mail inválido",
                    "InvalidUserName" => "Nome de usuário inválido",
                    _ => error.Description
                };

                // Todos os erros de senha vão para o campo "password"
                if (error.Code.StartsWith("Password"))
                {
                    errorMessages["password"] = message;
                }
                else if (error.Code.Contains("Email"))
                {
                    errorMessages["email"] = message;
                }
                else
                {
                    errorMessages["form"] = message;
                }
            }

            throw new FormException(errorMessages);
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
        user.Department = request.Department;
        
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

    /// <inheritdoc/>
    public async Task<List<UserCompetitionHistoryResponse>> GetUserCompetitionHistoryAsync(string userId)
    {
        var user = await this._userRepository
            .Query()
            .Include(u => u.Group)
                .ThenInclude(g => g.GroupInCompetitions)
                    .ThenInclude(gic => gic.Competition)
                        .ThenInclude(c => c.ExercisesInCompetition)
            .Include(u => u.Group)
                .ThenInclude(g => g.GroupExerciseAttempts)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user?.Group == null)
            return new List<UserCompetitionHistoryResponse>();

        var history = new List<UserCompetitionHistoryResponse>();

        // Get all competitions the user's group participated in
        var participatedCompetitions = user.Group.GroupInCompetitions
            .Where(gic => gic.Competition != null && 
                         gic.Competition.Status == CompetitionStatus.Finished)
            .Select(gic => gic.Competition)
            .Distinct()
            .ToList();

        foreach (var competition in participatedCompetitions)
        {
            // Get total exercises in the competition
            var totalExercises = competition.ExercisesInCompetition?.Count ?? 0;
            
            // Count solved exercises - exercises with at least one accepted submission
            var solvedExercises = user.Group.GroupExerciseAttempts
                .Where(attempt => 
                    attempt.CompetitionId == competition.Id && 
                    attempt.Accepted == true)
                .Select(attempt => attempt.ExerciseId)
                .Distinct()
                .Count();

            history.Add(new UserCompetitionHistoryResponse
            {
                Year = competition.StartTime.Year,
                GroupName = user.Group.Name,
                Questions = $"{solvedExercises}/{totalExercises}",
                CompetitionId = competition.Id,
                CompetitionName = competition.Name
            });
        }

        return history.OrderByDescending(h => h.Year).ToList();
    }
}
