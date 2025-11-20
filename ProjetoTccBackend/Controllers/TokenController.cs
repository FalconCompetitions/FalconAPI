using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjetoTccBackend.Services.Interfaces;

namespace ProjetoTccBackend.Controllers
{
    /// <summary>
    /// Controller responsible for managing access tokens.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class TokenController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly ILogger<TokenController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenController"/> class.
        /// </summary>
        /// <param name="tokenService">The service responsible for token operations.</param>
        /// <param name="logger">Logger for registering information and errors.</param>
        public TokenController(ITokenService tokenService, ILogger<TokenController> logger)
        {
            this._tokenService = tokenService;
            this._logger = logger;
        }

        /// <summary>
        /// Retrieves the current private access token used for teacher invitations.
        /// </summary>
        /// <remarks>
        /// Only users with the "Admin" role can access this endpoint.
        /// </remarks>
        /// <returns>The current private access token.</returns>
        /// <response code="200">Token successfully retrieved.</response>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetCurrentToken()
        {
            string? currentToken = this._tokenService.FetchPrivateAccessToken();

            return Ok(new { currentToken });
        }

        /// <summary>
        /// Generates and updates the teacher invitation token.
        /// </summary>
        /// <remarks>
        /// The generated token is valid for 1 day. Only users with the "Admin" role can access this endpoint.
        /// </remarks>
        /// <returns>The newly generated teacher invitation token.</returns>
        /// <response code="200">New token successfully generated.</response>
        [HttpPut]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> UpdateCurrentToken()
        {
            string newToken = this._tokenService.GenerateTeacherRoleInviteToken(TimeSpan.FromDays(1));

            return Ok(new { newToken });
        }
    }
}
