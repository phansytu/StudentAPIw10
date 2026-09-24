using Azure.Core;
using Microsoft.Identity.Client.Cache;
using StudentAPI.Application.Common.Interfaces;
using StudentAPI.Domain.Entities;

namespace StudentAPI.Infrastructure.Caching;

public class CachedSinhVienRepository : ISinhVienRepository
{
    private readonly ISinhVienRepository _repository;
    private readonly ICacheService _cache;

    private static string ListPrefix = "sinhvien:list:";

    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(30);

    public CachedSinhVienRepository(ISinhVienRepository repository, ICacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public Task<(List<SinhVien> Data, int TotalCount)> GetPagedAsync(
        string? keyWord,
        bool? gioiTinh,
        decimal? diemTu,
        decimal? diemDen,
        string? sortBy,
        bool descending,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var key = $"{ListPrefix}{keyWord}_{gioiTinh}_{diemTu}_{diemDen}_{sortBy}_{descending}_{pageIndex}_{pageSize}";
        return _cache.GetOrCreateAsync(
            key,
            () => _repository.GetPagedAsync(keyWord, gioiTinh, diemTu, diemDen, sortBy, descending, pageIndex, pageSize, cancellationToken),
            Ttl, cancellationToken)!;

    }

    public Task<SinhVien?> GetByIdAsync(int id, CancellationToken cancellationToken)

        => _cache.GetOrCreateAsync(
            $"sinhvien:id:{id}",
            () => _repository.GetByIdAsync(id, cancellationToken),
            Ttl, cancellationToken)!;

    public Task<SinhVien?> GetByMsvAsync(string maSV, CancellationToken cancellationToken)
    {
        return _cache.GetOrCreateAsync(
            $"sinhvien:msv:{maSV.Trim().ToLower()}",
            () => _repository.GetByMsvAsync(maSV, cancellationToken),
            Ttl, cancellationToken
        )!;
    }

    public async Task AddAsync(SinhVien student, CancellationToken cancellationToken)
    {
        await _repository.AddAsync(student, cancellationToken);
        InvalidateAll(student);

    }

    public async Task UpdateAsync(SinhVien student)
    {
        await _repository.UpdateAsync(student);
        InvalidateAll(student);
    }

    public async Task DeleteAsync(SinhVien student)
    {
        await _repository.DeleteAsync(student);
        InvalidateAll(student);
    }

    public Task<bool> ExistsByEmailAsync(string email, int? excludeId = null, CancellationToken cancellationToken = default)
    => _cache.GetOrCreateAsync(
            $"lophoc:email:{email.Trim().ToLower()}",
            () => _repository.ExistsByEmailAsync(email, excludeId, cancellationToken),
            Ttl, cancellationToken)!;
    private void InvalidateAll(SinhVien sinhVien)
    {
        _cache.RemoveByPrefix(ListPrefix);
        _cache.Remove($"sinhvien:id:{sinhVien.Id}");
        _cache.Remove($"lophoc:msv:{sinhVien.MaSV?.Trim().ToLower()}");
        _cache.Remove($"lophoc:email:{sinhVien.Email}");
    }
}