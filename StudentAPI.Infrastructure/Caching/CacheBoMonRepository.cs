// Infrastructure/Caching/CachedBoMonRepository.cs
using StudentAPI.Application.Common.Interfaces;
using StudentAPI.Domain.Entities;

namespace StudentAPI.Infrastructure.Caching;

public class CachedBoMonRepository : IBoMonRepository
{
    private readonly IBoMonRepository _repository;
    private readonly ICacheService _cache;

    private const string ListPrefix = "bomon:list:";
    private static readonly TimeSpan ListTtl = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan ItemTtl = TimeSpan.FromMinutes(30);

    public CachedBoMonRepository(IBoMonRepository decorated, ICacheService cache)
    {
        _repository = decorated;
        _cache = cache;
    }

    public Task<(List<BoMon> Data, int TotalCount)> GetAllAsync(
        int pageIndex, int pageSize, string? searchTerm, CancellationToken cancellationToken = default)
    {
        var key = $"{ListPrefix}{pageIndex}_{pageSize}_{searchTerm}";
        return _cache.GetOrCreateAsync(
            key,
            () => _repository.GetAllAsync(pageIndex, pageSize, searchTerm, cancellationToken),
            ListTtl,
            cancellationToken)!;
    }

    public Task<BoMon?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => _cache.GetOrCreateAsync(
            $"bomon:id:{id}",
            () => _repository.GetByIdAsync(id, cancellationToken),
            ItemTtl, cancellationToken)!;

    public Task<BoMon?> GetByMaBoMonAsync(string maBoMon, CancellationToken cancellationToken = default)
        => _cache.GetOrCreateAsync(
            $"bomon:mabomon:{maBoMon.Trim().ToLower()}",
            () => _repository.GetByMaBoMonAsync(maBoMon, cancellationToken),
            ItemTtl, cancellationToken)!;

    public Task<BoMon?> GetByTenBoMonAsync(string tenBoMon, CancellationToken cancellationToken = default)
        => _cache.GetOrCreateAsync(
            $"bomon:tenbomon:{tenBoMon.Trim().ToLower()}",
            () => _repository.GetByTenBoMonAsync(tenBoMon, cancellationToken),
            ItemTtl, cancellationToken)!;

    public async Task AddAsync(BoMon boMon, CancellationToken cancellationToken = default)
    {
        await _repository.AddAsync(boMon, cancellationToken);
        _cache.RemoveByPrefix(ListPrefix);
    }

    public void Update(BoMon boMon)
    {
        _repository.Update(boMon);
        _cache.RemoveByPrefix(ListPrefix);
        _cache.Remove($"bomon:id:{boMon.Id}");
        _cache.Remove($"bomon:mabomon:{boMon.MaBoMon?.Trim().ToLower()}");
        _cache.Remove($"bomon:tenbomon:{boMon.TenBoMon?.Trim().ToLower()}");
    }

    public void Delete(BoMon boMon)
    {
        _repository.Delete(boMon);
        _cache.RemoveByPrefix(ListPrefix);
        _cache.Remove($"bomon:id:{boMon.Id}");
        _cache.Remove($"bomon:mabomon:{boMon.MaBoMon?.Trim().ToLower()}");
        _cache.Remove($"bomon:tenbomon:{boMon.TenBoMon?.Trim().ToLower()}");

        _cache.Remove($"lophoc:bomon:{boMon.Id}");
    }
}