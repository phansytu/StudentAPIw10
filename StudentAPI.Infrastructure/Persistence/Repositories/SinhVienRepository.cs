using Microsoft.EntityFrameworkCore;
using StudentAPI.Application.Common.Interfaces;
using StudentAPI.Domain.Entities;

namespace StudentAPI.Infrastructure.Persistence.Repositories;

public class SinhVienRepository : ISinhVienRepository
{
    private readonly IAppDbContext _context;

    public SinhVienRepository(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<(List<SinhVien> Data, int TotalCount)> GetPagedAsync(
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

        var query = _context.SinhViens.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(keyWord))
        {
            var keywordLower = keyWord.Trim().ToLower();
            query = query.Where(x => x.MaSV!.ToLower().Contains(keywordLower)
                                  || x.HoTen!.ToLower().Contains(keywordLower));
        }

        if (gioiTinh.HasValue)
        {
            query = query.Where(x => x.GioiTinh == gioiTinh.Value);
        }

        if (diemTu.HasValue)
        {
            query = query.Where(x => x.DiemTB >= diemTu.Value);
        }

        if (diemDen.HasValue)
        {
            query = query.Where(x => x.DiemTB <= diemDen.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = (sortBy?.ToLower()) switch
        {
            "hoten" => descending ? query.OrderByDescending(x => x.HoTen) : query.OrderBy(x => x.HoTen),
            "diemtrungbinh" => descending ? query.OrderByDescending(x => x.DiemTB) : query.OrderBy(x => x.DiemTB),
            _ => descending ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id)
        };

        var data = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (data, totalCount);
    }
    public async Task<SinhVien?> GetByIdAsync(int id, CancellationToken cancellationToken)
            => await _context.SinhViens.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<SinhVien?> GetByMsvAsync(string maSV, CancellationToken cancellationToken)
        => await _context.SinhViens.AsNoTracking().FirstOrDefaultAsync(x => x.MaSV == maSV, cancellationToken);

    public async Task AddAsync(SinhVien student, CancellationToken cancellationToken)
        => await _context.SinhViens.AddAsync(student, cancellationToken);

    public Task UpdateAsync(SinhVien student)
    {
        _context.SinhViens.Update(student);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(SinhVien student)
    {
        _context.SinhViens.Remove(student);
        return Task.CompletedTask;
    }
    public async Task<bool> ExistsByEmailAsync(string email, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.SinhViens.AsNoTracking().Where(x => x.Email == email);

        if (excludeId.HasValue)
            query = query.Where(x => x.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }

}