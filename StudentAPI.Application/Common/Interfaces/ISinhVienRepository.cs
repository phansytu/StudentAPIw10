using StudentAPI.Application.Common.Models;
using StudentAPI.Domain.Entities;

namespace StudentAPI.Application.Common.Interfaces
{
    public interface ISinhVienRepository
    {
        Task<(List<SinhVien> Data, int TotalCount)> GetPagedAsync(
        string? keyWord,
        bool? gioiTinh,
        decimal? diemTu,
        decimal? diemDen,
        string? sortBy,
        bool descending,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken = default);
        Task<SinhVien?> GetByIdAsync(int id, CancellationToken cancellationToken);
        Task<SinhVien?> GetByMsvAsync(string maSV, CancellationToken cancellationToken);
        Task AddAsync(SinhVien student, CancellationToken cancellationToken);
        Task UpdateAsync(SinhVien student);
        Task DeleteAsync(SinhVien student);
        Task<bool> ExistsByEmailAsync(string email, int? excludeId = null, CancellationToken cancellationToken = default);


    }

}