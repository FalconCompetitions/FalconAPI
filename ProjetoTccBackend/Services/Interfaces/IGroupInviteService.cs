using ProjetoTccBackend.Database.Requests.Group;
using ProjetoTccBackend.Exceptions.Group;
using ProjetoTccBackend.Exceptions.User;
using ProjetoTccBackend.Models;

namespace ProjetoTccBackend.Services.Interfaces
{
    public interface IGroupInviteService
    {
        /// <summary>
        /// Sends a group invitation to a specified user.
        /// </summary>
        /// <remarks>This method creates a new group invitation for the specified user and group.  If the
        /// user is already part of a group, the operation will fail with a <see cref="UserHasGroupException"/>. If the
        /// group does not exist, the method will return <see langword="null"/>.</remarks>
        /// <param name="request">The request containing the user ID and group ID for the invitation.</param>
        /// <returns>A <see cref="GroupInvite"/> object representing the created invitation, or <see langword="null"/> if the
        /// specified group does not exist.</returns>
        /// <exception cref="UserNotFoundException">Thrown if the user specified in <paramref name="request"/> does not exist.</exception>
        /// <exception cref="UserHasGroupException">Thrown if the user specified in <paramref name="request"/> is already a member of a group.</exception>
        Task<GroupInvite?> SendGroupInviteToUser(InviteUserToGroupRequest request);


        /// <summary>
        /// Accepts a group invitation for the currently logged-in user.
        /// </summary>
        /// <remarks>This method updates the status of the group invitation to accepted and persists the
        /// changes to the database. The returned <see cref="GroupInvite"/> object includes the group details for
        /// further use.</remarks>
        /// <param name="groupId">The unique identifier of the group invitation to accept.</param>
        /// <returns>The accepted <see cref="GroupInvite"/> object, including the associated group details,  or <see
        /// langword="null"/> if the invitation does not belong to the logged-in user.</returns>
        /// <exception cref="GroupInvitationException">Thrown if the specified group invitation does not exist.</exception>
        Task<GroupInvite?> AcceptGroupInviteAsync(int groupId);
    }
}
