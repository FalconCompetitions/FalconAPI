using ProjetoTccBackend.Models;

namespace ProjetoTccBackend.Services.Interfaces;

/// <summary>
/// Service for managing the competition cache with centralized invalidation.
/// </summary>
public interface ICompetitionCacheService
{
    /// <summary>
    /// Gets the cached competition or fetches it from the database.
    /// </summary>
    Task<Competition?> GetOrFetchAsync(Func<Task<Competition?>> fetchFunc);

    /// <summary>
    /// Invalidates the competition cache.
    /// </summary>
    void InvalidateCache();
}
