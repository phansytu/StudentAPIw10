using Microsoft.EntityFrameworkCore;
using StudentAPI.Application.Common.Interfaces;
using StudentAPI.Domain.Entities;

namespace StudentAPI.Infrastructure.Persistence.Repositories;

public class LopHocRepository : ILopHocRepository
{
    private readonly IAppDbContext _context;

    public LopHocRepository(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<(List<LopHoc> Data, int TotalCount)> GetAllLopHocAsync(
        int pageIndex,
        int pageSize,
        string? searchTerm,
        int? boMonId,
        CancellationToken cancellationToken = default)
    {
        var query = _context.LopHocs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchKey = searchTerm.Trim().ToLower();
            query = query.Where(x => x.TenLop!.ToLower().Contains(searchKey));
        }
        if (boMonId.HasValue)
        {
            query = query.Where(x => x.BoMonId == boMonId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var data = await query
            .Include(x => x.BoMon)
            .OrderByDescending(x => x.Id)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (data, totalCount);
    }

    public async Task<LopHoc?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.LopHocs
            .AsNoTracking()
            .Include(x => x.BoMon)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<bool> IsMaLopUniqueAsync(string maLop, CancellationToken cancellationToken = default)
    {
        var exists = await _context.LopHocs
            .AsNoTracking()
            .AnyAsync(x => x.MaLop!.ToLower() == maLop.Trim().ToLower(), cancellationToken);

        return !exists;
    }

    public async Task AddAsync(LopHoc lopHoc, CancellationToken cancellationToken = default)
    {
        await _context.LopHocs.AddAsync(lopHoc, cancellationToken);
    }

    public Task UpdateAsync(LopHoc lopHoc)
    {
        _context.LopHocs.Update(lopHoc);
        return Task.CompletedTask;
    }

    public void Delete(LopHoc lopHoc)
    {
        _context.LopHocs.Remove(lopHoc);
    }
}