using MediatR;
using WhereIsTheTrain.Application.Common;
using WhereIsTheTrain.Application.Features.AdminAuth.DTOs;

namespace WhereIsTheTrain.Application.Features.AdminAuth.Commands;

public record AdminLoginCommand(string Email, string Password, bool RememberMe = false) : IRequest<Result<AdminAuthResponseDto>>;
public record AdminRefreshTokenCommand(string AccessToken, string RefreshToken) : IRequest<Result<AdminAuthResponseDto>>;
