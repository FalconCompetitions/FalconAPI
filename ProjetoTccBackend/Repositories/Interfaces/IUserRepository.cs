using ProjetoTccBackend.Models;

namespace ProjetoTccBackend.Repositories.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        public User? GetById(string id);
        public User? GetByEmail(string email);
        public User? GetByRa(string ra);


        /// <summary>
        /// Removes a user by ID.
        /// </summary>
        /// <param name="id">The user ID.</param>
        /// <returns>True if removed, false if not found.</returns>
        Task<bool> DeleteByIdAsync(string id);
    }
}
