using MediatR;
using WhereIsTheTrain.Application.Common;
using WhereIsTheTrain.Application.Features.Auth.DTOs;

namespace WhereIsTheTrain.Application.Features.Auth.Commands;

public record RegisterCommand(string DisplayName, string Email, string Password) : IRequest<Result<string>>;
public record ConfirmEmailCommand(string Token) : IRequest<Result<string>>;
public record LoginCommand(string Email, string Password, bool RememberMe = false) : IRequest<Result<AuthResponseDto>>;
public record RefreshTokenCommand(string AccessToken, string RefreshToken) : IRequest<Result<AuthResponseDto>>;
