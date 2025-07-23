using ProjetoTccBackend.Models;

namespace ProjetoTccBackend.Repositories.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        public User? GetById(string id);
        public User? GetByEmail(string email);
    }
}
