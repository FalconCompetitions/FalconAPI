using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
                UserName = user.UserName!,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            };

            return Ok(userResponse);
        }



        /// <summary>
        /// Retrieves a list of all users in the system.
        /// </summary>
        /// <returns>
        /// An <see cref="IActionResult"/> containing a list of <see cref="User"/> objects.
        /// </returns>
        /// <remarks>
        /// Accessible to users with the roles "Admin" or "Teacher".
        /// </remarks>
        /// <response code="200">Returns the list of users.</response>
        [Authorize(Roles = "Admin,Teacher")]
        [HttpGet()]
        public async Task<IActionResult> GetAllUsers()
        {
            List<User> users = await this._userService.GetAllUsers();

            return Ok(users);
        }


    }
}
