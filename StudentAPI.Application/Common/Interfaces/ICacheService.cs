namespace StudentAPI.Application.Common.Interfaces;

public interface ICacheService
{
    Task<T?> GetOrCreateAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default);

    void Remove(string key);
    void RemoveByPrefix(string prefix);
}