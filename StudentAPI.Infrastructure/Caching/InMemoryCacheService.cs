using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using StudentAPI.Application.Common.Interfaces;

namespace StudentAPI.Infrastructure.Caching;

public class InMemoryCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<InMemoryCacheService> _logger;
    private static readonly ConcurrentDictionary<string, byte> _keys = new();

    public InMemoryCacheService(IMemoryCache memoryCache, ILogger<InMemoryCacheService> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public async Task<T?> GetOrCreateAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        if (_memoryCache.TryGetValue(key, out T? cached))
        {
            _logger.LogInformation("[CACHE HIT] Key={Key}", key);
            return cached;
        }

        _logger.LogInformation("[CACHE MISS] Key={Key} -> Query nguồn dữ liệu gốc", key);

        var result = await factory();

        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(10)
        };

        options.RegisterPostEvictionCallback((k, v, reason, state) =>
        {
            _keys.TryRemove(k.ToString()!, out _);
            _logger.LogInformation("[CACHE EVICTED] Key={Key} Reason={Reason}", k, reason);
        });

        _memoryCache.Set(key, result, options);
        _keys.TryAdd(key, 0);

        return result;
    }

    public void Remove(string key)
    {
        _memoryCache.Remove(key);
        _keys.TryRemove(key, out _);
        _logger.LogInformation("[CACHE REMOVE] Key={Key}", key);
    }

    public void RemoveByPrefix(string prefix)
    {
        var keysToRemove = _keys.Keys.Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToList();
        foreach (var key in keysToRemove)
        {
            _memoryCache.Remove(key);
            _keys.TryRemove(key, out _);
        }
        _logger.LogInformation("[CACHE REMOVE BY PREFIX] Prefix={Prefix} Count={Count}", prefix, keysToRemove.Count);
    }
}