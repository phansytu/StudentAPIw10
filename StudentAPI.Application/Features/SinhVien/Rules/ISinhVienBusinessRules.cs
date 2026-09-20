namespace StudentAPI.Application.Features.SinhVien.Rules;

public interface ISinhVienBusinessRules
{
    Task KiemTraTrungEmail(string email, int? excludeId = null, CancellationToken cancellationToken = default);
}