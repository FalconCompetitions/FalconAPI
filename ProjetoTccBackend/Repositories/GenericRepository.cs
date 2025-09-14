using Microsoft.EntityFrameworkCore;
using ProjetoTccBackend.Database;
using ProjetoTccBackend.Repositories.Interfaces;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ProjetoTccBackend.Repositories
{
    /// <inheritdoc/>
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly TccDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the GenericRepository class.
        /// </summary>
        /// <param name="dbContext">The TccDbContext instance to be used by the repository.</param>
        public GenericRepository(TccDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public virtual void Add(T entity)
        {
            this._dbContext.Set<T>().Add(entity);
        }

        public virtual async Task AddAsync(T entity)
        {
            await this._dbContext.Set<T>().AddAsync(entity);
        }

        /// <inheritdoc/>
        public virtual void AddRange(IEnumerable<T> entities)
        {
            this._dbContext.Set<T>().AddRange(entities);
        }

        public virtual async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await this._dbContext.Set<T>().AddRangeAsync(entities);
        }

        /// <inheritdoc/>
        public virtual IEnumerable<T> Find(Expression<Func<T, bool>> expression)
        {
            return this._dbContext.Set<T>().Where(expression);
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> expression)
        {
            return await this._dbContext.Set<T>().Where(expression).ToListAsync();
        }

        /// <inheritdoc/>
        public virtual IEnumerable<T> GetAll()
        {
            return this._dbContext.Set<T>().ToList();
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await this._dbContext.Set<T>().ToListAsync();
        }

        /// <inheritdoc/>
        public virtual T? GetById(int id)
        {
            return this._dbContext.Set<T>().Find(id);
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await this._dbContext.Set<T>().FindAsync(id);
        }

        /// <inheritdoc/>
        public virtual T? GetById(string id)
        {
            return this._dbContext.Set<T>().Find(id);
        }

        public virtual async Task<T?> GetByIdAsync(string id)
        {
            return await this._dbContext.Set<T>().FindAsync(id);
        }

        /// <inheritdoc/>
        public virtual void Remove(T entity)
        {
            this._dbContext.Set<T>().Remove(entity);
        }

        /// <inheritdoc/>
        public virtual void RemoveRange(IEnumerable<T> entities)
        {
            
            this._dbContext.Set<T>().RemoveRange(entities);
        }

        /// <inheritdoc/>
        public virtual void Update(T entity)
        {
            this._dbContext.Set<T>().Update(entity);
        }

        /// <inheritdoc/>
        public virtual void UpdateRange(IEnumerable<T> entities)
        {
            this._dbContext.Set<T>().UpdateRange(entities);
        }
    }
}
