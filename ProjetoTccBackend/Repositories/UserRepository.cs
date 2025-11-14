using Microsoft.EntityFrameworkCore;
using ProjetoTccBackend.Database;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Repositories.Interfaces;

namespace ProjetoTccBackend.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(TccDbContext dbContext)
            : base(dbContext) { }

        public override IEnumerable<User> GetAll()
        {
            return this._dbContext.Users.Include(x => x.Group);
        }

        public User? GetByEmail(string email)
        {
            return this
                ._dbContext.Users.Include(x => x.Group)
                .Where(x => x.Email!.ToLower() == email.ToLower())
                .FirstOrDefault();
        }

        public User? GetByRa(string ra)
        {
            return this
                ._dbContext.Users.Include(x => x.Group)
                .Where(x => x.RA.Equals(ra))
                .FirstOrDefault();
        }

        public User? GetById(string id)
        {
            return this
                ._dbContext.Users.Include(x => x.Group)
                .Where(x => x.Id.Equals(id))
                .FirstOrDefault();
        }

        public async Task<bool> DeleteByIdAsync(string id)
        {
            var user = await this._dbContext.Users.FindAsync(id);
            if (user == null)
                return false;
            this._dbContext.Users.Remove(user);
            await this._dbContext.SaveChangesAsync();
            return true;
        }
    }
}
