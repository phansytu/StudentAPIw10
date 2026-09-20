using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using StudentAPI.Application.Common.Interfaces;
using StudentAPI.Domain.Entities;

namespace StudentAPI.Infrastructure.Persistence.Repositories;

public class BoMonRepository : IBoMonRepository
{
    private readonly IAppDbContext _context;

    public BoMonRepository(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<(List<BoMon> Data, int TotalCount)> GetAllAsync(
        int pageIndex,
        int pageSize,
        string? searchTerm,
        CancellationToken cancellationToken = default)
    {
        var query = _context.BoMons.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(x => x.MaBoMon!.ToLower().Contains(term) ||
                                     x.TenBoMon!.ToLower().Contains(term));
        }
        var totalCount = await query.CountAsync(cancellationToken);

        var data = await query
            .OrderByDescending(x => x.Id)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (data, totalCount);
    }

    public async Task<BoMon?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.BoMons
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<BoMon?> GetByMaBoMonAsync(string maBoMon, CancellationToken cancellationToken = default)
    {
        return await _context.BoMons
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaBoMon!.ToLower() == maBoMon.Trim().ToLower(), cancellationToken);
    }

    public async Task<BoMon?> GetByTenBoMonAsync(string tenBoMon, CancellationToken cancellationToken = default)
    {
        return await _context.BoMons
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenBoMon!.ToLower() == tenBoMon.Trim().ToLower(), cancellationToken);
    }

    public async Task AddAsync(BoMon boMon, CancellationToken cancellationToken = default)
    {
        await _context.BoMons.AddAsync(boMon, cancellationToken);
    }

    public void Update(BoMon boMon)
    {
        _context.BoMons.Update(boMon);
    }

    public void Delete(BoMon boMon)
    {
        _context.BoMons.Remove(boMon);
    }

}