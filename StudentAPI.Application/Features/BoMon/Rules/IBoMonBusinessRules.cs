namespace StudentAPI.Application.Features.BoMon.Rules;

public interface IBoMonBusinessRules
{
    Task KiemTraTenMon(string maSV, int? excludeId = null, CancellationToken cancellationToken = default);
}