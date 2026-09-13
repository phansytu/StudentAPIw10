using Microsoft.EntityFrameworkCore;
using StudentAPI.Application.Common.Interfaces;
using StudentAPI.Domain.Entities;

namespace StudentAPI.Infrastructure;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<SinhVien> SinhViens => Set<SinhVien>();
    public DbSet<LopHoc> LopHocs => Set<LopHoc>();
    public DbSet<BoMon> BoMons => Set<BoMon>();
    public DbSet<NguoiDung> NguoiDungs => Set<NguoiDung>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
}