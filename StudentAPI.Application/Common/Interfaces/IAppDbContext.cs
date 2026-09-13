using StudentAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace StudentAPI.Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<BoMon> BoMons { get; }
    DbSet<SinhVien> SinhViens { get; }
    DbSet<LopHoc> LopHocs { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<NguoiDung> NguoiDungs { get; }
}