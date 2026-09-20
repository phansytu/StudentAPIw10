using AutoMapper;
using MediatR;
using StudentAPI.Application.Common.Interfaces;
using StudentAPI.Application.Features.SinhVien.Rules;
namespace StudentAPI.Application.Features.SinhVien.Commands.TaoSinhVien;

public class TaoSinhVienHandler : IRequestHandler<TaoSinhVienCommand, int>
{
    private readonly ISinhVienRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ISinhVienBusinessRules _businessRules;

    public TaoSinhVienHandler(
        ISinhVienRepository repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ISinhVienBusinessRules businessRules)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _businessRules = businessRules;
    }
    public async Task<int> Handle(TaoSinhVienCommand request, CancellationToken cancellationToken)
    {
        await _businessRules.KiemTraTrungEmail(request.Email, excludeId: null, cancellationToken);
        var sinhVien = _mapper.Map<Domain.Entities.SinhVien>(request);
        await _repository.AddAsync(sinhVien, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return sinhVien.Id;
    }
}

