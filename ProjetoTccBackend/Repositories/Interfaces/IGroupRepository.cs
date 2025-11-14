using ProjetoTccBackend.Models;

namespace ProjetoTccBackend.Repositories.Interfaces
{
    public interface IGroupRepository : IGenericRepository<Group>
    {
        /// <summary>
        /// Gets a group by ID with its users included.
        /// </summary>
        /// <param name="id">The group ID.</param>
        /// <returns>The group with users, or null if not found.</returns>
        Group? GetByIdWithUsers(int id);
    }
}
