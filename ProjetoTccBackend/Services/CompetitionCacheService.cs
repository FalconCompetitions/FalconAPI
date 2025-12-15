using Microsoft.Extensions.Caching.Memory;
using ProjetoTccBackend.Models;
using ProjetoTccBackend.Services.Interfaces;

namespace ProjetoTccBackend.Services;

/// <summary>
/// Implementation of the competition cache service.
/// </summary>
public class CompetitionCacheService : ICompetitionCacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<CompetitionCacheService> _logger;
    private const string CompetitionCacheKey = "currentCompetition";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Initializes a new instance of the <see cref="CompetitionCacheService"/> class.
    /// </summary>
    /// <param name="memoryCache">The memory cache instance.</param>
    /// <param name="logger">The logger instance.</param>
    public CompetitionCacheService(
        IMemoryCache memoryCache,
        ILogger<CompetitionCacheService> logger
    )
    {
        _memoryCache = memoryCache;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Competition?> GetOrFetchAsync(Func<Task<Competition?>> fetchFunc)
    {
        if (_memoryCache.TryGetValue(CompetitionCacheKey, out Competition? competition))
        {
            _logger.LogDebug("Competition cache hit");
            return competition;
        }

        _logger.LogDebug("Competition cache miss - fetching from database");
        competition = await fetchFunc();

        if (competition is not null)
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(
                CacheDuration
            );

            _memoryCache.Set(CompetitionCacheKey, competition, cacheEntryOptions);
            _logger.LogInformation(
                "Cached competition {CompetitionId} - {CompetitionName} for {Duration}s",
                competition.Id,
                competition.Name,
                CacheDuration.TotalSeconds
            );
        }

        return competition;
    }

    /// <inheritdoc />
    public void InvalidateCache()
    {
        _memoryCache.Remove(CompetitionCacheKey);
        _logger.LogInformation("Competition cache invalidated");
    }
}
