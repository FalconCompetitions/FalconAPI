using ProjetoTccBackend.Models;
using ProjetoTccBackend.Database.Requests.Group;
using ProjetoTccBackend.Database.Responses.Global;
using ProjetoTccBackend.Database.Responses.Group;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ProjetoTccBackend.Services.Interfaces
{
    /// <summary>
    /// Service responsible for managing group-related operations, including creation, update, retrieval, and membership management.
    /// Handles business logic for group entities and coordinates with repositories and user services.
    /// </summary>
    public interface IGroupService
    {
        /// <summary>
        /// Creates a new group and associates it with the logged-in user.
        /// </summary>
        /// <param name="groupRequest">An object containing the details of the group to be created.</param>
        /// <returns>
        /// A <see cref="Group"/> object representing the newly created group.
        /// </returns>
        /// <exception cref="UnauthorizedAccessException">
        /// Thrown if the logged-in user is not authenticated.
        /// </exception>
        Task<Group> CreateGroupAsync(CreateGroupRequest groupRequest);


        /// <summary>
        /// Changes the name of an existing group.
        /// </summary>
        /// <param name="groupRequest">An object containing the group ID and the new name for the group.</param>
        /// <returns>
        /// A <see cref="Group"/> object with the updated name, or <c>null</c> if the group does not exist.
        /// </returns>
        /// <exception cref="UnauthorizedAccessException">
        /// Thrown if the logged-in user is not authorized to change the name of the specified group.
        /// </exception>
        Group? ChangeGroupName(ChangeGroupNameRequest groupRequest);


        /// <summary>
        /// Retrieves a group by its ID if the logged-in user has access to it.
        /// </summary>
        /// <param name="id">The ID of the group to retrieve.</param>
        /// <returns>
        /// A <see cref="Group"/> object representing the group with the specified ID, 
        /// or <c>null</c> if the group does not exist.
        /// </returns>
        /// <exception cref="UnauthorizedAccessException">
        /// Thrown if the logged-in user does not have access to the specified group.
        /// </exception>
        Group? GetGroupById(int id);

        /// <summary>
        /// Retrieves a paginated list of groups associated with the logged-in user, with optional search functionality.
        /// </summary>
        /// <param name="page">The page number to retrieve.</param>
        /// <param name="pageSize">The number of groups per page.</param>
        /// <param name="search">An optional search term to filter groups by name.</param>
        /// <returns>
        /// A task that represents the asynchronous operation, with a <see cref="PagedResult{Group}"/> containing the paginated list of groups.
        /// </returns>
        /// <exception cref="UnauthorizedAccessException">
        /// Thrown if the logged-in user is not authenticated.
        /// </exception>
        Task<PagedResult<GroupResponse>> GetGroupsAsync(int page, int pageSize, string? search);

        /// <summary>
        /// Updates a group's information and its users, validating permissions.
        /// </summary>
        /// <param name="groupId">The ID of the group to update.</param>
        /// <param name="request">The update request data.</param>
        /// <param name="userId">The id of the user making the request.</param>
        /// <param name="userRoles">The roles of the user making the request.</param>
        /// <returns>The updated group, or null if not found or not allowed.</returns>
        Task<GroupResponse?> UpdateGroupAsync(int groupId, UpdateGroupRequest request, string userId, IList<string> userRoles);

        /// <summary>
        /// Deletes a group by its ID, validating permissions.
        /// </summary>
        /// <param name="groupId">The ID of the group to delete.</param>
        /// <param name="userId">The ID of the user making the request.</param>
        /// <param name="userRoles">The roles of the user making the request.</param>
        /// <returns>True if successfully deleted, false if not found or not allowed.</returns>
        Task<bool> DeleteGroupAsync(int groupId, string userId, IList<string> userRoles);
    }
}
