using Microsoft.Extensions.Caching.Memory;
using Npgsql;

namespace OpenCand.Repository
{
    public abstract class BaseRepository
    {
        protected string ConnectionString { get; private set; }
        protected NpgsqlConnection? Connection { get; private set; }
        protected readonly IMemoryCache? _cache;

        // Default cache settings
        protected static readonly TimeSpan DefaultCacheExpiration = TimeSpan.FromDays(7);
        protected static readonly CacheItemPriority DefaultCachePriority = CacheItemPriority.Normal;

        public BaseRepository(IConfiguration configuration, IMemoryCache? cache = null)
        {
            ConnectionString = configuration["DatabaseSettings:ConnectionString"] ?? 
                throw new ArgumentNullException("Connection string not found in configuration");
            _cache = cache;
        }
        
        /// <summary>
        /// Generic method to get data from cache or execute a factory function if not cached
        /// </summary>
        /// <typeparam name="T">Type of data to cache</typeparam>
        /// <param name="cacheKey">Unique cache key</param>
        /// <param name="factory">Function to execute if data is not in cache</param>
        /// <param name="expiration">Cache expiration time (optional, uses default if not provided)</param>
        /// <param name="priority">Cache priority (optional, uses default if not provided)</param>
        /// <returns>Cached or freshly retrieved data</returns>
        protected async Task<T?> GetOrSetCacheAsync<T>(
            string cacheKey,
            Func<Task<T?>> factory,
            TimeSpan? expiration = null,
            CacheItemPriority? priority = null) where T : class
        {
            // If caching is not available, execute factory directly
            if (_cache == null)
            {
                return await factory();
            }

            // Try to get cached data first
            if (_cache.TryGetValue(cacheKey, out T? cachedData) && cachedData != null)
            {
                return cachedData;
            }

            // If not in cache, execute factory function
            var result = await factory();
            
            // Only cache non-null results
            if (result != null)
            {
                _cache.Set(cacheKey, result, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = expiration ?? DefaultCacheExpiration,
                    Priority = priority ?? DefaultCachePriority
                });
            }

            return result;
        }

        /// <summary>
        /// Generic method to get data from cache or execute a synchronous factory function if not cached
        /// </summary>
        /// <typeparam name="T">Type of data to cache</typeparam>
        /// <param name="cacheKey">Unique cache key</param>
        /// <param name="factory">Function to execute if data is not in cache</param>
        /// <param name="expiration">Cache expiration time (optional, uses default if not provided)</param>
        /// <param name="priority">Cache priority (optional, uses default if not provided)</param>
        /// <returns>Cached or freshly retrieved data</returns>
        protected T? GetOrSetCache<T>(
            string cacheKey,
            Func<T?> factory,
            TimeSpan? expiration = null,
            CacheItemPriority? priority = null) where T : class
        {
            // If caching is not available, execute factory directly
            if (_cache == null)
            {
                return factory();
            }

            // Try to get cached data first
            if (_cache.TryGetValue(cacheKey, out T? cachedData) && cachedData != null)
            {
                return cachedData;
            }

            // If not in cache, execute factory function
            var result = factory();
            
            // Only cache non-null results
            if (result != null)
            {
                _cache.Set(cacheKey, result, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = expiration ?? DefaultCacheExpiration,
                    Priority = priority ?? DefaultCachePriority
                });
            }

            return result;
        }

        /// <summary>
        /// Removes an item from cache by key
        /// </summary>
        /// <param name="cacheKey">Cache key to remove</param>
        protected void ClearCache(string cacheKey)
        {
            _cache?.Remove(cacheKey);
        }

        /// <summary>
        /// Checks if an item exists in cache
        /// </summary>
        /// <param name="cacheKey">Cache key to check</param>
        /// <returns>True if item exists in cache, false otherwise</returns>
        protected bool IsCached(string cacheKey)
        {
            return _cache?.TryGetValue(cacheKey, out _) ?? false;
        }        /// <summary>
        /// Generates a standardized cache key for entity-based caching
        /// </summary>
        /// <param name="entityName">Name of the entity (e.g., "Candidato", "Stats")</param>
        /// <param name="identifier">Unique identifier for the entity (optional)</param>
        /// <returns>Formatted cache key</returns>
        protected static string GenerateCacheKey(string entityName, object? identifier = null)
        {
            return identifier != null ? $"{entityName}_{identifier}" : entityName;
        }
    }
}
