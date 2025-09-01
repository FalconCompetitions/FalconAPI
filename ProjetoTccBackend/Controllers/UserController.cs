using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProjetoTccBackend.Database.Requests.User;
using ProjetoTccBackend.Database.Responses.Auth;
using ProjetoTccBackend.Database.Responses.User;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Services.Interfaces;

namespace ProjetoTccBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private IUserService _userService;

        public UserController(IUserService userService)
        {
            this._userService = userService;
        }

        /// <summary>
        /// Retrieves user information based on the provided user ID.
        /// </summary>
        /// <param name="userId">The ID of the user to retrieve information for.</param>
        /// <returns>An IActionResult containing the user information.</returns>
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserInfo(string userId)
        {
            User? user = await this._userService.GetUser(userId);

            if (user is null)
            {
                return NotFound(userId);
            }

            var userResponse = new UserInfoResponse()
            {
                Email = user.Email!,
                Id = user.Id,
                JoinYear = user.JoinYear,
                PhoneNumber = user.PhoneNumber,
                RA = user.RA,
                Name = user.UserName!,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            };

            return Ok(userResponse);
        }

        /// <summary>
        /// Retrieves a paginated list of users in the system.
        /// </summary>
        /// <param name="page">The page number to retrieve.</param>
        /// <param name="pageSize">The number of users per page.</param>
        /// <param name="search">Optional. A search term to filter users by name or email.</param>
        /// <param name="role">Optional. A role to filter users by.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing a paginated list of users.
        /// </returns>
        /// <remarks>
        /// Accessible to users with the roles "Admin" or "Teacher".
        /// </remarks>
        /// <response code="200">Returns the paginated list of users.</response>
        [Authorize(Roles = "Admin,Teacher")]
        [HttpGet()]
        public async Task<IActionResult> GetAllUsers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? role = null
        )
        {
            var result = await this._userService.GetUsersAsync(page, pageSize, search, role);
            return Ok(result);
        }

        /// <summary>
        /// Updates the profile data of a user. Only accessible by Admin or the user themselves.
        /// </summary>
        /// <param name="userId">The ID of the user to update.</param>
        /// <param name="request">The update request data.</param>
        /// <returns>The updated user object, or NotFound if not found, or Forbid if not allowed.</returns>
        [Authorize]
        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateUser(
            string userId,
            [FromBody] UpdateUserRequest request
        )
        {
            var loggedUser = User;
            var isAdmin = loggedUser.IsInRole("Admin");
            var loggedUserId = loggedUser.Claims.FirstOrDefault(c => c.Type.Equals("id"))?.Value;
            if (!isAdmin && loggedUserId != userId)
            {
                return Forbid();
            }
            var updatedUser = await this._userService.UpdateUserAsync(userId, request);
            if (updatedUser == null)
            {
                return NotFound(userId);
            }
            return Ok(updatedUser);
        }
    }
}
