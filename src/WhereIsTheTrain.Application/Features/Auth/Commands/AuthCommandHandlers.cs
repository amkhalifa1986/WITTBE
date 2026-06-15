using MediatR;
using WhereIsTheTrain.Application.Common;
using WhereIsTheTrain.Application.Interfaces;
using WhereIsTheTrain.Domain.Entities;
using WhereIsTheTrain.Domain.Interfaces;
using BCrypt.Net;

namespace WhereIsTheTrain.Application.Features.Auth.Commands;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<string>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;

    public RegisterCommandHandler(IUnitOfWork unitOfWork, IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
    }

    public async Task<Result<string>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _unitOfWork.Users.GetByEmailAsync(request.Email, cancellationToken);
        if (existingUser != null)
            return Result<string>.Failure("A user with this email already exists.", 409);

        var confirmationToken = Guid.NewGuid().ToString("N");

        var user = new User
        {
            DisplayName = request.DisplayName,
            Email = request.Email.ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            EmailConfirmationToken = confirmationToken,
            EmailConfirmed = false
        };

        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _emailService.SendEmailConfirmationAsync(user.Email, user.DisplayName, confirmationToken, cancellationToken);

        return Result<string>.Success("Registration successful. Please check your email to confirm your account.", 201);
    }
}

public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, Result<string>>
{
    private readonly IUnitOfWork _unitOfWork;

    public ConfirmEmailCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<string>> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByEmailConfirmationTokenAsync(request.Token, cancellationToken);
        if (user == null)
            return Result<string>.Failure("Invalid or expired confirmation token.", 400);

        if (user.EmailConfirmed)
            return Result<string>.Success("Email already confirmed.");

        user.EmailConfirmed = true;
        user.EmailConfirmationToken = null;
        await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<string>.Success("Email confirmed successfully. You can now log in.");
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<Features.Auth.DTOs.AuthResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;

    public LoginCommandHandler(IUnitOfWork unitOfWork, IJwtService jwtService)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
    }

    public async Task<Result<DTOs.AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[DEBUG] Login attempt for email: '{request.Email}'");
        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email.ToLowerInvariant(), cancellationToken);
        if (user == null) {
            Console.WriteLine($"[DEBUG] User '{request.Email}' not found in DB.");
            return Result<DTOs.AuthResponseDto>.Failure("Invalid email or password.", 401);
        }
        
        if (string.IsNullOrEmpty(user.PasswordHash)) {
            Console.WriteLine($"[DEBUG] User '{request.Email}' has empty password hash.");
            return Result<DTOs.AuthResponseDto>.Failure("Invalid email or password.", 401);
        }

        bool passValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        Console.WriteLine($"[DEBUG] Password check for '{request.Email}': {passValid}");
        if (!passValid) {
            Console.WriteLine($"[DEBUG] Supplied Password length: {request.Password?.Length}. DB Hash length: {user.PasswordHash.Length}");
            return Result<DTOs.AuthResponseDto>.Failure("Invalid email or password.", 401);
        }

        // if (!user.EmailConfirmed)
        //    return Result<DTOs.AuthResponseDto>.Failure("Please confirm your email before logging in.", 403);

        if (user.IsSuspended)
            return Result<DTOs.AuthResponseDto>.Failure("Your account has been suspended. Please contact support.", 403);

        var accessToken = _jwtService.GenerateAccessToken(user, request.RememberMe);
        var refreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = request.RememberMe ? DateTime.UtcNow.AddDays(7) : DateTime.UtcNow.AddDays(1);
        await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<DTOs.AuthResponseDto>.Success(new DTOs.AuthResponseDto
        {
            UserId = user.Id,
            DisplayName = user.DisplayName,
            Email = user.Email,
            Role = user.Role.ToString(),
            AccessToken = accessToken,
            RefreshToken = refreshToken
        });
    }
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<DTOs.AuthResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;

    public RefreshTokenCommandHandler(IUnitOfWork unitOfWork, IJwtService jwtService)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
    }

    public async Task<Result<DTOs.AuthResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var userId = _jwtService.ValidateAccessToken(request.AccessToken);
        if (userId == null)
            return Result<DTOs.AuthResponseDto>.Failure("Invalid access token.", 401);

        var user = await _unitOfWork.Users.GetByIdAsync(userId.Value, cancellationToken);
        if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            return Result<DTOs.AuthResponseDto>.Failure("Invalid or expired refresh token.", 401);

        if (user.IsSuspended)
            return Result<DTOs.AuthResponseDto>.Failure("Your account has been suspended. Please contact support.", 403);

        var newAccessToken = _jwtService.GenerateAccessToken(user);
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<DTOs.AuthResponseDto>.Success(new DTOs.AuthResponseDto
        {
            UserId = user.Id,
            DisplayName = user.DisplayName,
            Email = user.Email,
            Role = user.Role.ToString(),
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        });
    }
}
