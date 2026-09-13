using MediatR;
using Microsoft.EntityFrameworkCore;
using StudentAPI.Application.Common.Interfaces;
using StudentAPI.Application.DTOs;
using StudentAPI.Application.Features.Auth.Commands.Login;
using StudentAPI.Domain.Entities;



public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IAppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtGenerator;
    private readonly IUnitOfWork _unitOfWork;

    public LoginCommandHandler(IAppDbContext context, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtGenerator, IUnitOfWork unitOfWork)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtGenerator = jwtGenerator;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.NguoiDungs
            .FirstOrDefaultAsync(x => x.Email == request.Email, cancellationToken);

        if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Email hoặc mật khẩu không chính xác.");

        var (accessToken, expiresAt) = _jwtGenerator.GenerateAccessToken(user);

        var refreshTokenStr = _jwtGenerator.GenerateRefreshToken();
        var refreshToken = new RefreshToken
        {
            Token = refreshTokenStr,
            NguoiDungId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };

        _context.RefreshTokens.Add(refreshToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto(accessToken, refreshTokenStr, expiresAt);
    }
}