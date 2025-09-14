using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ProjetoTccBackend.Repositories.Interfaces
{
    /// <summary>
    /// Defines a generic repository interface for CRUD operations.
    /// </summary>
    /// <typeparam name="T">The type of entity being managed by the repository.</typeparam>
    public interface IGenericRepository<T> where T : class
    {
        /// <summary>
        /// Adds a new entity to the repository.
        /// </summary>
        /// <param name="entity">The entity to be added.</param>
        void Add(T entity);

        /// <summary>
        /// Adds a collection of new entities to the repository.
        /// </summary>
        /// <param name="entities">The collection of entities to be added.</param>
        void AddRange(IEnumerable<T> entities);

        /// <summary>
        /// Retrieves all entities from the repository.
        /// </summary>
        /// <returns>A collection of all entities.</returns>
        IEnumerable<T> GetAll();

        /// <summary>
        /// Retrieves an entity by its ID.
        /// </summary>
        /// <param name="id">The ID of the entity to be retrieved.</param>
        /// <returns>The entity with the specified ID, or null if not found.</returns>
        T GetById(int id);

        /// <summary>
        /// Retrieves entities that match the specified filter expression.
        /// </summary>
        /// <param name="expression">The filter expression.</param>
        /// <returns>A collection of entities that match the filter expression.</returns>
        IEnumerable<T> Find(Expression<Func<T, bool>> expression);

        /// <summary>
        /// Removes an entity from the repository.
        /// </summary>
        /// <param name="entity">The entity to be removed.</param>
        void Remove(T entity);

        /// <summary>
        /// Removes a collection of entities from the repository.
        /// </summary>
        /// <param name="entities">The collection of entities to be removed.</param>
        void RemoveRange(IEnumerable<T> entities);

        /// <summary>
        /// Updates an entity in the repository.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        void Update(T entity);

        /// <summary>
        /// Updates a collection of entities in the repository.
        /// </summary>
        /// <param name="entities">The collection of entities to be updated.</param>
        void UpdateRange(IEnumerable<T> entities);

        /// <summary>
        /// Asynchronously adds a new entity to the repository.
        /// </summary>
        /// <param name="entity">The entity to be added.</param>
        Task AddAsync(T entity);

        /// <summary>
        /// Asynchronously adds a collection of new entities to the repository.
        /// </summary>
        /// <param name="entities">The collection of entities to be added.</param>
        Task AddRangeAsync(IEnumerable<T> entities);

        /// <summary>
        /// Asynchronously retrieves all entities from the repository.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation, containing a collection of all entities.</returns>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Asynchronously retrieves an entity by its ID.
        /// </summary>
        /// <param name="id">The ID of the entity to be retrieved.</param>
        /// <returns>A task that represents the asynchronous operation, containing the entity with the specified ID, or null if not found.</returns>
        Task<T?> GetByIdAsync(int id);

        /// <summary>
        /// Asynchronously retrieves an entity by its ID.
        /// </summary>
        /// <param name="id">The ID of the entity to be retrieved.</param>
        /// <returns>A task that represents the asynchronous operation, containing the entity with the specified ID, or null if not found.</returns>
        Task<T?> GetByIdAsync(string id);

        /// <summary>
        /// Asynchronously retrieves entities that match the specified filter expression.
        /// </summary>
        /// <param name="expression">The filter expression.</param>
        /// <returns>A task that represents the asynchronous operation, containing a collection of entities that match the filter expression.</returns>
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> expression);
    }
}
