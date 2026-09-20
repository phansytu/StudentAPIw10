using StudentAPI.Domain.Entities;

namespace StudentAPI.Application.Common.Interfaces;

public interface IBoMonRepository
{
    Task<(List<BoMon> Data, int TotalCount)> GetAllAsync(
        int pageIndex,
        int pageSize,
        string? searchTerm,
        CancellationToken cancellationToken = default);

    Task<BoMon?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<BoMon?> GetByMaBoMonAsync(string maBoMon, CancellationToken cancellationToken = default);

    Task<BoMon?> GetByTenBoMonAsync(string tenBoMon, CancellationToken cancellationToken = default);

    Task AddAsync(BoMon boMon, CancellationToken cancellationToken = default);

    void Update(BoMon boMon);

    void Delete(BoMon boMon);
}