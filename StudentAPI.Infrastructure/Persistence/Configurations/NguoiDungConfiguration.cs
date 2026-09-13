using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentAPI.Domain.Entities;

namespace StudentAPI.Infrastructure.Persistence.Configurations;

public class NguoiDungConfiguration : IEntityTypeConfiguration<NguoiDung>
{
    public void Configure(EntityTypeBuilder<NguoiDung> builder)
    {
        builder.ToTable("NguoiDung");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(150);

        builder.HasIndex(x => x.Email)
            .IsUnique();

        builder.Property(x => x.PasswordHash)
            .IsRequired();

        builder.Property(x => x.HoTen)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Role)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasMany(x => x.RefreshTokens)
            .WithOne(x => x.NguoiDung)
            .HasForeignKey(x => x.NguoiDungId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}