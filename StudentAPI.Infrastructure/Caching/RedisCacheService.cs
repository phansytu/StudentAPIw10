using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using StudentAPI.Application.Common.Interfaces;
using System.Text.Json;

namespace StudentAPI.Infrastructure.Caching;

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public RedisCacheService(
        IDistributedCache cache,
        IConnectionMultiplexer redis,
        ILogger<RedisCacheService> logger)
    {
        _cache = cache;
        _redis = redis;
        _logger = logger;
    }

    public async Task<T?> GetOrCreateAsync<T>(
    string key,
    Func<Task<T>> factory,
    TimeSpan? expiration = null,
    CancellationToken cancellationToken = default)
    {
        var cachedJson = await _cache.GetStringAsync(key, cancellationToken);

        if (cachedJson is not null)
        {
            _logger.LogInformation("[REDIS HIT] Key={Key}", key);
            return JsonSerializer.Deserialize<T>(cachedJson, _jsonOptions);
        }

        _logger.LogInformation("[REDIS MISS] Key={Key} -> Query nguồn dữ liệu gốc", key);

        var result = await factory();

        var json = JsonSerializer.Serialize(result, _jsonOptions);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(10)
        };

        await _cache.SetStringAsync(key, json, options, cancellationToken);

        return result;
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
        _logger.LogInformation("[REDIS REMOVE] Key={Key}", key);
    }

    public void RemoveByPrefix(string prefix)
    {
        var endpoint = _redis.GetEndPoints().First();
        var server = _redis.GetServer(endpoint);
        var db = _redis.GetDatabase();

        var keys = server.Keys(pattern: $"{prefix}*").ToArray();

        if (keys.Length == 0)
        {
            _logger.LogInformation("[REDIS REMOVE BY PREFIX] Prefix={Prefix} - Không có key nào", prefix);
            return;
        }

        db.KeyDelete(keys);
        _logger.LogInformation("[REDIS REMOVE BY PREFIX] Prefix={Prefix} Count={Count}", prefix, keys.Length);
    }
}