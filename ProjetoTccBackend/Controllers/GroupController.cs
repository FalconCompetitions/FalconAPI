using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjetoTccBackend.Database.Requests.Group;
using ProjetoTccBackend.Database.Responses.Global;
using ProjetoTccBackend.Database.Responses.Group;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupController"/> class.
        /// </summary>
        /// <param name="groupService">The service responsible for group operations.</param>
        public GroupController(IGroupService groupService)
        {
            this._groupService = groupService;
        }

        /// <summary>
        /// Creates a new group.
        /// </summary>
        /// <param name="request">The details of the group to be created.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing the ID and name of the created group.
        /// </returns>
        /// <remarks>
        /// Accessible to users with the roles "Admin", "Teacher", or "Student".
        /// </remarks>
        /// <response code="200">Returns the ID and name of the created group.</response>
        /// <response code="400">If the request is invalid.</response>
        [Authorize(Roles = "Admin,Teacher,Student")]
        [HttpPost()]
        [ProducesResponseType(typeof(Group), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateGroup([FromBody] CreateGroupRequest request)
        {
            var group = await this._groupService.CreateGroupAsync(request);
            return Ok(new
            {
                group.Id,
                group.Name
            });
        }

        /// <summary>
        /// Retrieves a group by its ID.
        /// </summary>
        /// <param name="id">The ID of the group to retrieve.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing the group details if found, 
        /// or a <see cref="NotFoundResult"/> if the group does not exist.
        /// </returns>
        /// <remarks>
        /// Accessible to users with the roles "Admin", "Teacher", or "Student".
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

            return Ok(group);
        }

        /// <summary>
        /// Retrieves a paginated list of groups.
        /// </summary>
        /// <param name="page">The page number to retrieve.</param>
        /// <param name="pageSize">The number of groups per page.</param>
        /// <param name="search">Optional. A search term to filter groups by name.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing a paginated list of groups.
        /// </returns>
        /// <remarks>
        /// Accessible to users with the roles "Admin" or "Teacher".
        /// </remarks>
        /// <response code="200">Returns the paginated list of groups.</response>
        [Authorize(Roles = "Admin,Teacher")]
        [HttpGet()]
        [ProducesResponseType(typeof(PagedResult<GroupResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetGroups([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            var result = await this._groupService.GetGroupsAsync(page, pageSize, search);
            return Ok(result);
        }

    }
}
