using StudentAPI.Application.Common.Exceptions;
using StudentAPI.Application.Common.Interfaces;

namespace StudentAPI.Application.Features.SinhVien.Rules;

public class SinhVienBusinessRules : ISinhVienBusinessRules
{
    private readonly ISinhVienRepository _repository;

    public SinhVienBusinessRules(ISinhVienRepository repository)
    {
        _repository = repository;
    }

    public async Task KiemTraTrungEmail(string email, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var exists = await _repository.ExistsByEmailAsync(email, excludeId, cancellationToken);
        if (exists)
            throw new BusinessException($"Email '{email}' đã được sử dụng");
    }

}