using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentAPI.Application.Common.Interfaces;
using StudentAPI.Application.DTOs;
using StudentAPI.Domain.Entities;

namespace StudentAPI.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
{
    private readonly IAppDbContext _context;
    private readonly IJwtTokenGenerator _jwtGenerator;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshTokenCommandHandler(IAppDbContext context, IJwtTokenGenerator jwtGenerator, IUnitOfWork unitOfWork)
    {
        _context = context;
        _jwtGenerator = jwtGenerator;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // 1. Trích xuất thông tin User từ AccessToken cũ
        var principal = _jwtGenerator.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal == null)
            throw new UnauthorizedAccessException("AccessToken không hợp lệ.");

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier) ?? principal.FindFirst("sub");
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            throw new UnauthorizedAccessException("Token không chứa UserId hợp lệ.");

        // 2. Kiểm tra RefreshToken trong DB
        var savedRefreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == request.RefreshToken && x.NguoiDungId == userId, cancellationToken);

        if (savedRefreshToken == null || savedRefreshToken.IsRevoked || savedRefreshToken.ExpiresAt <= DateTime.UtcNow)
            throw new UnauthorizedAccessException("RefreshToken không hợp lệ hoặc đã hết hạn.");

        // 3. Thu hồi RefreshToken cũ (Rotation)
        savedRefreshToken.IsRevoked = true;

        // 4. Lấy thông tin User & tạo cặp Token mới
        var user = await _context.NguoiDungs.FindAsync(new object[] { userId }, cancellationToken);
        if (user == null) throw new UnauthorizedAccessException("Không tìm thấy người dùng.");

        var (newAccessToken, expiresAt) = _jwtGenerator.GenerateAccessToken(user);
        var newRefreshTokenStr = _jwtGenerator.GenerateRefreshToken();

        _context.RefreshTokens.Add(new Domain.Entities.RefreshToken
        {
            Token = newRefreshTokenStr,
            NguoiDungId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        });

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto(newAccessToken, newRefreshTokenStr, expiresAt);
    }
}