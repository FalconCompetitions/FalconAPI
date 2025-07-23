using ProjetoTccBackend.Database;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Repositories.Interfaces;

namespace ProjetoTccBackend.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(TccDbContext dbContext) : base(dbContext)
        {
            
        }


        public User? GetByEmail(string email)
        {
            return this._dbContext.Set<User>().Where(x => x.Email!.ToLower() == email.ToLower()).FirstOrDefault();
        }

        public User? GetById(string id)
        {
            return this._dbContext.Set<User>().Find(id);
        }

    }
}
