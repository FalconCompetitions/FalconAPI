using ProjetoTccBackend.Models;

namespace ProjetoTccBackend.Repositories.Interfaces
{
    public interface IGroupInviteRepository : IGenericRepository<GroupInvite>
    {

        /// <summary>
        /// Determines whether a user belongs to a group by their user ID.
        /// </summary>
        /// <remarks>This method queries the database to determine the group membership of the specified
        /// user. The result includes the group details if the user is found in a group, or <see langword="null"/> if no
        /// group is associated with the user.</remarks>
        /// <param name="userId">The unique identifier of the user to check. Cannot be null or empty.</param>
        /// <returns>A <see cref="Group"/> object representing the group the user belongs to,  or <see langword="null"/> if the
        /// user is not part of any group.</returns>
        Task<Group?> IsUserInGroupById(string userId);


        /// <summary>
        /// Retrieves a collection of users associated with the specified group ID.
        /// </summary>
        /// <remarks>This method performs an asynchronous query to retrieve users associated with the
        /// specified group ID.  The query includes related user data and ensures that the returned collection is fully
        /// populated.</remarks>
        /// <param name="groupId">The unique identifier of the group whose users are to be retrieved.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of  <see
        /// cref="User"/> objects associated with the specified group. If no users are found, the collection will be
        /// empty.</returns>
        Task<ICollection<User>> GetUsersInGroupById(int groupId);
    }
}
