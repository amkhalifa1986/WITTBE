using MediatR;
using WhereIsTheTrain.Application.Common;
using WhereIsTheTrain.Application.Interfaces;
using WhereIsTheTrain.Application.Features.AdminAuth.DTOs;
using WhereIsTheTrain.Domain.Entities;
using WhereIsTheTrain.Domain.Interfaces;
using BCrypt.Net;

namespace WhereIsTheTrain.Application.Features.AdminAuth.Commands;

public class AdminLoginCommandHandler : IRequestHandler<AdminLoginCommand, Result<AdminAuthResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;

    public AdminLoginCommandHandler(IUnitOfWork unitOfWork, IJwtService jwtService)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
    }

    public async Task<Result<AdminAuthResponseDto>> Handle(AdminLoginCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.ToLowerInvariant().Trim();
        var admin = await _unitOfWork.AdminUsers.GetByEmailWithRoleAsync(email, cancellationToken);

        if (admin == null || !BCrypt.Net.BCrypt.Verify(request.Password, admin.PasswordHash))
        {
            return Result<AdminAuthResponseDto>.Failure("Invalid email or password.", 401);
        }

        var accessToken = _jwtService.GenerateAdminAccessToken(admin, request.RememberMe);
        var refreshToken = _jwtService.GenerateRefreshToken();

        admin.RefreshToken = refreshToken;
        admin.RefreshTokenExpiryTime = request.RememberMe ? DateTime.UtcNow.AddDays(7) : DateTime.UtcNow.AddDays(1);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<AdminAuthResponseDto>.Success(new AdminAuthResponseDto
        {
            AdminId = admin.Id,
            DisplayName = admin.DisplayName,
            Email = admin.Email,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            IsSuperAdmin = admin.IsSuperAdmin
        });
    }
}

public class AdminRefreshTokenCommandHandler : IRequestHandler<AdminRefreshTokenCommand, Result<AdminAuthResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;

    public AdminRefreshTokenCommandHandler(IUnitOfWork unitOfWork, IJwtService jwtService)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
    }

    public async Task<Result<AdminAuthResponseDto>> Handle(AdminRefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var adminId = _jwtService.ValidateAccessToken(request.AccessToken);
        if (adminId == null)
            return Result<AdminAuthResponseDto>.Failure("Invalid access token.", 401);

        var admin = await _unitOfWork.AdminUsers.GetByIdWithRoleAsync(adminId.Value, cancellationToken);

        if (admin == null || admin.RefreshToken != request.RefreshToken || admin.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return Result<AdminAuthResponseDto>.Failure("Invalid or expired refresh token.", 401);
        }

        var newAccessToken = _jwtService.GenerateAdminAccessToken(admin);
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        admin.RefreshToken = newRefreshToken;
        admin.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<AdminAuthResponseDto>.Success(new AdminAuthResponseDto
        {
            AdminId = admin.Id,
            DisplayName = admin.DisplayName,
            Email = admin.Email,
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            IsSuperAdmin = admin.IsSuperAdmin
        });
    }
}
