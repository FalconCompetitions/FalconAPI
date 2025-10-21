using ApiEstoqueASP.Services;
using Microsoft.AspNetCore.Mvc;
using ProjetoTccBackend.Filters;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Services.Interfaces;
using ProjetoTccBackend.Exceptions;
using ProjetoTccBackend.Database.Requests.Auth;
using ProjetoTccBackend.Database.Responses.Auth;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace ProjetoTccBackend.Controllers
{
    /// <summary>
    /// Controlador responsável pela autenticação de usuários.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly IUserService _userService;
        private readonly ILogger<AuthController> _logger;

        /// <summary>
        /// Construtor do AuthController.
        /// </summary>
        /// <param name="tokenService">Serviço de geração de token.</param>
        /// <param name="userService">Serviço de gerenciamento de usuários.</param>
        /// <param name="logger">Logger para registrar informações e erros.</param>
        public AuthController(ITokenService tokenService, IUserService userService, ILogger<AuthController> logger)
        {
            this._tokenService = tokenService;
            this._userService = userService;
            this._logger = logger;
        }

        /// <summary>
        /// Sets the authentication cookie for the user.
        /// </summary>
        /// <param name="token">The authentication token to be set in the cookie.</param>
        /// <remarks>
        /// This function sets a secure cookie named "CompetitionAuthToken" with the provided token.
        /// The cookie is set with HttpOnly, Secure, and SameSite flags to ensure its security.
        /// </remarks>
        [ApiExplorerSettings(IgnoreApi = true)]
        public void SetAuthCookie(string token)
        {
            CookieOptions cookieOptions = new CookieOptions()
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Path = "/",
            };

            this.Response.Cookies.Append("CompetitionAuthToken", token, cookieOptions);
        }

        /// <summary>
        /// Registers a new user in the system.
        /// </summary>
        /// <param name="request">The <see cref="RegisterUserRequest"/> object containing the user's registration details.</param>
        /// <returns>
        /// Returns an <see cref="IActionResult"/> containing the registered user's details and a JWT token.
        /// On success, returns a 200 OK response with the user and token.
        /// On failure, returns appropriate error responses such as 400 Bad Request or 500 Internal Server Error.
        /// </returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/Auth/register
        ///     {
        ///         "ra": "12345",
        ///         "userName": "JohnDoe",
        ///         "email": "johndoe@example.com",
        ///         "joinYear": 2023,
        ///         "accessCode": "optionalCode",
        ///         "role": "User",
        ///         "password": "SecurePassword123"
        ///     }
        /// 
        /// Sample response:
        /// 
        ///     {
        ///         "user": {
        ///             "id": "userId",
        ///             "userName": "JohnDoe",
        ///             "email": "johndoe@example.com",
        ///             "emailConfirmed": false,
        ///             "joinYear": 2023,
        ///             "phoneNumber": null,
        ///             "phoneNumberConfirmed": false
        ///         },
        ///         "token": "jwtTokenString"
        ///     }
        /// </remarks>
        /// <response code="200">Returns the registered user and token</response>
        /// <response code="400">If the request is invalid</response>
        /// <response code="500">If an internal server error occurs</response>
        [HttpPost("register")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterUserRequest request)
        {
            var result = await this._userService.RegisterUserAsync(request);

            this._logger.LogDebug("dasd");

            User user = result.Item1;
            string role = result.Item2;

            string jwtToken = this._tokenService.GenerateUserToken(user, request.Role);

            UserResponse userResponse = new UserResponse()
            {
                Id = user.Id,
                Email = user.Email!,
                RA = user.RA,
                Group = null,
                EmailConfirmed = user.EmailConfirmed,
                Name = user.UserName!,
                JoinYear = user.JoinYear,
                PhoneNumber = user.PhoneNumber,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                Role = role,
            };

            SetAuthCookie(jwtToken);

            return Ok(new
            {
                user = userResponse,
                token = jwtToken
            });
        }


        /// <summary>
        /// Authenticates a user using their email and password.
        /// </summary>
        /// <param name="request">The <see cref="LoginUserRequest"/> object containing the user's login credentials.</param>
        /// <returns>
        /// Returns an <see cref="IActionResult"/> containing the authenticated user's details and a JWT token.
        /// On success, returns a 200 OK response with the user and token.
        /// On failure, returns appropriate error responses such as 400 Bad Request or 500 Internal Server Error.
        /// </returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/Auth/login
        ///     {
        ///         "email": "johndoe@example.com",
        ///         "password": "SecurePassword123"
        ///     }
        /// 
        /// Sample response:
        /// 
        ///     {
        ///         "user": {
        ///             "id": "userId",
        ///             "userName": "JohnDoe",
        ///             "email": "johndoe@example.com",
        ///             "emailConfirmed": false,
        ///             "joinYear": 2023,
        ///             "phoneNumber": null,
        ///             "phoneNumberConfirmed": false
        ///         },
        ///         "token": "jwtTokenString"
        ///     }
        /// </remarks>
        /// <response code="200">Returns the authenticated user and token</response>
        /// <response code="400">If the request is invalid</response>
        /// <response code="500">If an internal server error occurs</response>
        [HttpPost("login")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> LoginUser([FromBody] LoginUserRequest request)
        {
            Tuple<User, string> result = await this._userService.LoginUserAsync(request);

            User user = result.Item1;
            string role = result.Item2;

            UserResponse userResponse = new UserResponse()
            {
                Id = user.Id,
                RA = user.RA,
                Email = user.Email!,
                EmailConfirmed = user.EmailConfirmed,
                Name = user.Name!,
                JoinYear = user.JoinYear,
                PhoneNumber = user.PhoneNumber,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                Role = role,
                Group = user.Group,
            };

            string jwtToken = this._tokenService.GenerateUserToken(user, role);

            SetAuthCookie(jwtToken);

            return Ok(new
            {
                user = userResponse,
                token = jwtToken
            });
        }


        /// <summary>
        /// Validates the authentication token of the current user.
        /// </summary>
        /// <returns>
        /// Returns an <see cref="IActionResult"/> containing a boolean indicating whether the token is valid.
        /// </returns>
        /// <remarks>
        /// Exemplo de uso:
        /// <code>
        ///     GET /api/auth/validate
        /// </code>
        /// </remarks>
        /// <response code="200">Returns a valid token response.</response>
        [HttpGet("validate")]
        [Authorize]
        [ProducesResponseType(200)]
        public IActionResult ValidateToken()
        {
            string? token = this.Request.Cookies.FirstOrDefault(x => x.Key == "CompetitionAuthToken").Value;

            if (token is null)
            {
                return Ok(new { valid = false });
            }

            return Ok(new { valid = true });
        }


        /// <summary>
        /// Renews the authentication token for the currently logged in user.
        /// </summary>
        /// <returns>
        /// Returns an IActionResult containing the renewed authentication token.
        /// On success, returns a 200 OK response with the renewed token.
        /// On failure, returns an Unauthorized response if the user is not found.
        /// </returns>
        /// <remarks>
        /// This function is accessible only to authorized users.
        /// </remarks>
        /// <response code="200">Returns the renewed authentication token.</response>
        /// <response code="401">If the user is not found.</response>
        [HttpPost("renew")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> RenewToken()
        {
            var loggedUser = this._userService.GetHttpContextLoggedUser();

            if (loggedUser is null)
            {
                return Unauthorized(new { valid = false, message = "User not found" });
            }

            var role = this.User.Claims.First(x => x.Subject.NameClaimType == ClaimTypes.Role)!.Value;

            string jwtToken = this._tokenService.GenerateUserToken(loggedUser, role);

            SetAuthCookie(jwtToken);

            return Ok(new
            {
                valid = true,
                token = jwtToken,
            });
        }


        /// <summary>
        /// Logs out the currently authenticated user.
        /// </summary>
        /// <remarks>
        /// This method terminates the user's session by performing the necessary logout operations and deleting the authentication token cookie. The user must be authenticated to call this method.<br/>
        /// Exemplo de uso:
        /// <code>
        ///     GET /api/auth/logout
        /// </code>
        /// </remarks>
        /// <returns>A <see cref="NoContentResult"/> indicating that the logout operation was successful.</returns>
        /// <response code="200">If the user is logged out successfully.</response>
        [HttpGet("logout")]
        [Authorize]
        public async Task<IActionResult> LogoutUser()
        {
            await this._userService.LogoutAsync();

            this.Response.Cookies.Delete("CompetitionAuthToken");
            return Ok();
        }

    }
}
