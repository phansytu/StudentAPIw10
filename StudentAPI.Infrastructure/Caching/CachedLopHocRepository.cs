using StudentAPI.Application.Common.Interfaces;
using StudentAPI.Domain.Entities;

namespace StudentAPI.Infrastructure.Caching;

public class CachedLopHocRepository : ILopHocRepository
{
    private readonly ILopHocRepository _decorated;
    private readonly ICacheService _cache;

    private const string ListPrefix = "lophoc:list:";
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(30);

    public CachedLopHocRepository(ILopHocRepository decorated, ICacheService cache)
    {
        _decorated = decorated;
        _cache = cache;
    }

    public Task<(List<LopHoc> Data, int TotalCount)> GetAllLopHocAsync(
        int pageIndex, int pageSize, string? searchTerm, int? boMonId, CancellationToken cancellationToken = default)
    {
        var key = $"{ListPrefix}{pageIndex}_{pageSize}_{searchTerm}_{boMonId}";
        return _cache.GetOrCreateAsync(
            key,
            () => _decorated.GetAllLopHocAsync(pageIndex, pageSize, searchTerm, boMonId, cancellationToken),
            Ttl, cancellationToken)!;
    }

    public Task<LopHoc?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => _cache.GetOrCreateAsync(
            $"lophoc:id:{id}",
            () => _decorated.GetByIdAsync(id, cancellationToken),
            Ttl, cancellationToken)!;

    public Task<bool> IsMaLopUniqueAsync(string maLop, CancellationToken cancellationToken = default)
        => _cache.GetOrCreateAsync(
            $"lophoc:malop:{maLop.Trim().ToLower()}",
            () => _decorated.IsMaLopUniqueAsync(maLop, cancellationToken),
            Ttl, cancellationToken)!;



    public async Task AddAsync(LopHoc lopHoc, CancellationToken cancellationToken = default)
    {
        await _decorated.AddAsync(lopHoc, cancellationToken);
        _cache.RemoveByPrefix(ListPrefix);
        _cache.Remove($"lophoc:bomon:{lopHoc.BoMonId}");
    }

    public async Task UpdateAsync(LopHoc lopHoc)
    {
        await _decorated.UpdateAsync(lopHoc);
        InvalidateAll(lopHoc);
    }

    public void Delete(LopHoc lopHoc)
    {
        _decorated.Delete(lopHoc);
        InvalidateAll(lopHoc);
    }

    private void InvalidateAll(LopHoc lopHoc)
    {
        _cache.RemoveByPrefix(ListPrefix);
        _cache.Remove($"lophoc:id:{lopHoc.Id}");
        _cache.Remove($"lophoc:malop:{lopHoc.MaLop?.Trim().ToLower()}");
        _cache.Remove($"lophoc:bomon:{lopHoc.BoMonId}"); // Xóa cache danh sách lớp theo bộ môn cha
    }


}