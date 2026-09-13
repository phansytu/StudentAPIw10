using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentAPI.Application.Common.Interfaces;

namespace StudentAPI.Application.Features.NguoiDung.Commands.TaoNguoiDung;

public class TaoNguoiDungCommandHandler : IRequestHandler<TaoNguoiDungCommand, int>
{
    private readonly IAppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public TaoNguoiDungCommandHandler(IAppDbContext context, IPasswordHasher passwordHasher, IUnitOfWork unitOfWork)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(TaoNguoiDungCommand request, CancellationToken cancellationToken)
    {

        var isEmailExist = await _context.NguoiDungs
            .AnyAsync(x => x.Email == request.Email, cancellationToken);

        if (isEmailExist)
            throw new InvalidOperationException("Email này đã được sử dụng.");

        var nguoiDung = new Domain.Entities.NguoiDung
        {
            Email = request.Email,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            HoTen = request.HoTen,
            Role = string.IsNullOrWhiteSpace(request.Role) ? "SinhVien" : request.Role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.NguoiDungs.Add(nguoiDung);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return nguoiDung.Id;
    }
}