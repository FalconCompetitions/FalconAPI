using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjetoTccBackend.Database.Requests.Group;
using ProjetoTccBackend.Database.Responses.Competition;
using ProjetoTccBackend.Database.Responses.Global;
using ProjetoTccBackend.Database.Responses.Group;
using ProjetoTccBackend.Database.Responses.User;
using ProjetoTccBackend.Exceptions.Group;
using ProjetoTccBackend.Exceptions.User;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Services.Interfaces;

namespace ProjetoTccBackend.Controllers
{
    /// <summary>
    /// Controller responsible for managing groups.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class GroupController : ControllerBase
    {
        private readonly IGroupService _groupService;
        private readonly IGroupInviteService _groupInviteService;
        private readonly IUserService _userService;
        private readonly IGroupInCompetitionService _groupInCompetitionService;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupController"/> class.
        /// </summary>
        /// <param name="groupService">The service responsible for group operations.</param>
        /// <param name="groupInviteService">The service responsible for group invitation</param>
        /// <param name="userService">The service responsible for user operations.</param>
        /// <param name="groupInCompetitionService">The service responsible for group-in-competition operations.</param>
        public GroupController(
            IGroupService groupService,
            IGroupInviteService groupInviteService,
            IUserService userService,
            IGroupInCompetitionService groupInCompetitionService
        )
        {
            this._groupService = groupService;
            this._groupInviteService = groupInviteService;
            this._userService = userService;
            this._groupInCompetitionService = groupInCompetitionService;
        }

        /// <summary>
        /// Creates a new group.
        /// </summary>
        /// <param name="request">The details of the group to be created.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing the ID and name of the created group.
        /// </returns>
        /// <remarks>
        /// Accessible to users with the roles "Admin", "Teacher", or "Student".<br/>
        /// Exemplo de request:
        /// <code>
        ///     POST /api/group
        ///     {
        ///         "name": "Grupo 1"
        ///     }
        /// </code>
        /// </remarks>
        /// <response code="200">Returns the ID and name of the created group.</response>
        /// <response code="400">If the request is invalid.</response>
        [Authorize(Roles = "Admin,Teacher,Student")]
        [HttpPost()]
        [ProducesResponseType(typeof(Group), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateGroup([FromBody] CreateGroupRequest request)
        {
            try
            {
                Group group = await this._groupService.CreateGroupAsync(request);

                GroupResponse response = new GroupResponse()
                {
                    Id = group.Id,
                    Name = group.Name,
                    LeaderId = group.LeaderId,
                    Users = group
                        .Users.Select(u => new GenericUserInfoResponse()
                        {
                            Id = u.Id,
                            Name = u.Name,
                            CreatedAt = u.CreatedAt,
                            Department = u.Department,
                            Email = u.Email,
                            Group = null,
                            ExercisesCreated = null,
                            JoinYear = u.JoinYear,
                            LastLoggedAt = u.LastLoggedAt,
                            Ra = u.RA,
                        })
                        .ToList(),
                    GroupInvitations = group
                        .GroupInvites.Select(g => new GroupInvitationResponse()
                        {
                            Id = g.Id,
                            User = new GenericUserInfoResponse()
                            {
                                Id = g.User.Id,
                                CreatedAt = g.User.CreatedAt,
                                Department = g.User.Department,
                                Email = g.User.Email,
                                Group = null,
                                ExercisesCreated = null,
                                JoinYear = g.User.JoinYear,
                                LastLoggedAt = g.User.LastLoggedAt,
                                Name = g.User.Name,
                                Ra = g.User.RA,
                            },
                            Accepted = false,
                        })
                        .ToList(),
                };

                return CreatedAtAction(nameof(GetGroupById), new { id = response.Id }, response);
            }
            catch (UserHasGroupException ex)
            {
                return BadRequest(new { ex.Message });
            }
        }

        /// <summary>
        /// Retrieves a group by its ID.
        /// </summary>
        /// <param name="id">The ID of the group to retrieve.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing the group details if found, or a <see cref="NotFoundResult"/> if the group does not exist.
        /// </returns>
        /// <remarks>
        /// Accessible to users with the roles "Admin", "Teacher", or "Student".<br/>
        /// Exemplo de uso:
        /// <code>
        ///     GET /api/group/1
        /// </code>
        /// </remarks>
        /// <response code="200">Returns the group details.</response>
        /// <response code="404">If the group is not found.</response>
        [Authorize(Roles = "Admin,Teacher,Student")]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Group), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetGroupById(int id)
        {
            Group? group = this._groupService.GetGroupById(id);

            if (group is null)
            {
                return NotFound(id);
            }

            GroupResponse response = new GroupResponse()
            {
                Id = group.Id,
                Name = group.Name,
                LeaderId = group.LeaderId,
                Users = group
                    .Users.Select(u => new GenericUserInfoResponse()
                    {
                        Id = u.Id,
                        Name = u.Name,
                        CreatedAt = u.CreatedAt,
                        Department = u.Department,
                        Email = u.Email,
                        Group = null,
                        ExercisesCreated = null,
                        JoinYear = u.JoinYear,
                        LastLoggedAt = u.LastLoggedAt,
                        Ra = u.RA,
                    })
                    .ToList(),
                GroupInvitations = [],
            };

            return Ok(response);
        }

        /// <summary>
        /// Retrieves a paginated list of groups.
        /// </summary>
        /// <param name="page">The page number to retrieve. Default is 1.</param>
        /// <param name="pageSize">The number of groups per page. Default is 10.</param>
        /// <param name="search">Optional. A search term to filter groups by name.</param>
        /// <returns>An <see cref="IActionResult"/> containing a paginated list of groups.</returns>
        /// <remarks>
        /// Accessible to users with the roles "Admin" or "Teacher".<br/>
        /// Exemplo de uso:
        /// <code>
        ///     GET /api/group?page=1&pageSize=10&search=grupo
        /// </code>
        /// </remarks>
        /// <response code="200">Returns the paginated list of groups.</response>
        [Authorize(Roles = "Admin,Teacher")]
        [HttpGet()]
        [ProducesResponseType(typeof(PagedResult<GroupResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetGroups(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null
        )
        {
            var result = await this._groupService.GetGroupsAsync(page, pageSize, search);
            return Ok(result);
        }

        /// <summary>
        /// Updates a group's information and its users.
        /// </summary>
        /// <param name="id">The ID of the group to update.</param>
        /// <param name="request">The update request data, including the new name and the list of user IDs to associate with the group.</param>
        /// <returns>The updated group if successful. Returns <see cref="ForbidResult"/> if the user does not have permission, or <see cref="NotFoundResult"/> if the group does not exist.</returns>
        /// <remarks>
        /// Accessible to users with the roles "Admin", "Teacher" ou membros do grupo.<br/>
        /// Exemplo de request:
        /// <code>
        ///     PUT /api/group/1
        ///     {
        ///         "name": "Novo Nome do Grupo",
        ///         "userIds": ["userId1", "userId2"]
        ///     }
        /// </code>
        /// </remarks>
        /// <response code="200">Returns the updated group.</response>
        /// <response code="403">If the user does not have permission to update the group.</response>
        /// <response code="404">If the group is not found.</response>
        [Authorize(Roles = "Admin,Teacher,Student")]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateGroup(int id, [FromBody] UpdateGroupRequest request)
        {
            var loggedUserId = User.Claims.FirstOrDefault(c => c.Type.Equals("id"))?.Value;
            var userRoles = User
                .Claims.Where(c => c.Type.Equals("role"))
                .Select(c => c.Value)
                .ToList();
            try
            {
                var updatedGroup = await this._groupService.UpdateGroupAsync(
                    id,
                    request,
                    loggedUserId,
                    userRoles
                );
                if (updatedGroup == null)
                    return Forbid();
                return Ok(updatedGroup);
            }
            catch (GroupConcurrencySuccessException ex)
            {
                return Ok(new { success = true, message = ex.Message });
            }
        }

        [HttpGet("invite")]
        [Authorize]
        public async Task<IActionResult> GetGroupInvitations()
        {
            User loggedUser = this._userService.GetHttpContextLoggedUser();

            List<GroupInvite> invitations = await this._groupInviteService.GetUserGroupInvites(
                loggedUser.Id
            );

            return Ok(invitations);
        }

        /// <summary>
        /// Sends an invitation to a user to join a specified group.
        /// </summary>
        /// <remarks>This action is restricted to users with the "Student" role. Ensure the request body
        /// contains valid data for the invitation.</remarks>
        /// <param name="request">The request containing the details of the invitation, including the user ID and group ID.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation. Returns: <list type="bullet">
        /// <item><description><see cref="StatusCodes.Status201Created"/> if the invitation is successfully
        /// created.</description></item> <item><description><see cref="StatusCodes.Status400BadRequest"/> if the group
        /// does not exist, the user is not found, or the user is already part of a group.</description></item> </list></returns>
        [Authorize(Roles = "Student")]
        [HttpPost("invite")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> InviteUserToGroup(
            [FromBody] InviteUserToGroupRequest request
        )
        {
            try
            {
                GroupInvite? groupInvite = await this._groupInviteService.SendGroupInviteToUser(
                    request
                );

                if (groupInvite is null)
                {
                    return BadRequest($"O grupo de id {request.GroupId} não existe!");
                }

                GroupInvitationResponse response = new GroupInvitationResponse()
                {
                    Id = groupInvite.Id,
                    Accepted = groupInvite.Accepted,
                    Group = new GroupResponse()
                    {
                        Id = groupInvite.Group.Id,
                        LeaderId = groupInvite.Group.LeaderId,
                        Name = groupInvite.Group.Name,
                    },
                    User = new GenericUserInfoResponse()
                    {
                        Id = groupInvite.User.Id,
                        Name = groupInvite.User.Name,
                        Group = null,
                        Email = groupInvite.User.Email,
                        CreatedAt = groupInvite.User.CreatedAt,
                        Department = null,
                        ExercisesCreated = null,
                        JoinYear = groupInvite.User.JoinYear,
                        LastLoggedAt = groupInvite.User.LastLoggedAt,
                        Ra = groupInvite.User.RA,
                    },
                };

                return CreatedAtAction(
                    nameof(InviteUserToGroup),
                    new { userId = request.RA },
                    response
                );
            }
            catch (UserNotFoundException exc)
            {
                return BadRequest(new { exc.Message });
            }
            catch (UserHasGroupException exc)
            {
                return BadRequest(new { exc.Message });
            }
        }



        /// <summary>
        /// Removes the specified user from the specified group.
        /// </summary>
        /// <remarks>This method requires the caller to have the necessary permissions to remove a user
        /// from the group.</remarks>
        /// <param name="groupId">The unique identifier of the group from which the user will be removed.</param>
        /// <param name="userId">The unique identifier of the user to be removed from the group.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation: <list type="bullet">
        /// <item><description><see cref="OkResult"/> if the user was successfully removed from the
        /// group.</description></item> <item><description><see cref="ForbidResult"/> if the operation is not
        /// permitted.</description></item> <item><description><see cref="BadRequestObjectResult"/> if the operation
        /// could not be completed due to invalid input or other errors.</description></item> </list></returns>
        [Authorize(Roles = "Student")]
        [HttpDelete("{groupId}/exit/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ExitFromGroup(int groupId, string userId)
        {
            bool? res = await this._groupInviteService.RemoveUserFromGroupAsync(groupId, userId);

            if (res == null)
            {
                return BadRequest(new { res });
            }

            if (res == false)
            {
                return Forbid();
            }

            return Ok();
        }

        /// <summary>
        /// Accepts a group invitation for the currently authenticated user.
        /// </summary>
        /// <remarks>This action requires the user to be authenticated and have the "Student"
        /// role.</remarks>
        /// <param name="groupId">The unique identifier of the group whose invitation is being accepted.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.  Returns <see cref="OkObjectResult"/>
        /// with the result of the acceptance if successful,  or <see cref="BadRequestObjectResult"/> if the operation
        /// fails.</returns>
        /// <remarks>
        /// Request example:
        /// <code>
        ///     PUT /api/group/accept/1
        ///     {}
        /// </code>
        /// </remarks>
        [Authorize(Roles = "Student")]
        [HttpPut("accept/{groupId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AcceptGroupInvite(int groupId)
        {
            try
            {
                var res = await this._groupInviteService.AcceptGroupInviteAsync(groupId);

                if (res is null)
                {
                    return BadRequest();
                }

                return Ok(res);
            }
            catch (GroupInvitationException ex)
            {
                return BadRequest(new { ex.Message });
            }
        }

        /// <summary>
        /// Retrieves the current valid competition registration for a group.
        /// </summary>
        /// <remarks>
        /// This endpoint returns the competition registration that is currently active for the specified group.
        /// A registration is considered valid if the current date and time fall within the competition's start and end times.
        /// The group must not be blocked from participating in the competition.
        /// Accessible to users with the roles "Admin", "Teacher", or "Student".
        /// </remarks>
        /// <param name="groupId">The unique identifier of the group.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing the current valid competition registration if found,
        /// or <see cref="NotFoundResult"/> if no valid registration exists.
        /// </returns>
        /// <response code="200">Returns the current valid competition registration for the group.</response>
        /// <response code="404">If no valid competition registration is found for the group.</response>
        [Authorize(Roles = "Admin,Teacher,Student")]
        [HttpGet("{groupId}/current-competition")]
        [ProducesResponseType(typeof(GroupInCompetitionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCurrentValidCompetitionByGroupId(int groupId)
        {
            var groupInCompetition = await this._groupInCompetitionService
                .GetCurrentValidCompetitionByGroupIdAsync(groupId);

            if (groupInCompetition is null)
            {
                return NotFound(new { message = $"Nenhuma competição ativa encontrada para o grupo de id {groupId}" });
            }

            return Ok(groupInCompetition);
        }
    }
}
