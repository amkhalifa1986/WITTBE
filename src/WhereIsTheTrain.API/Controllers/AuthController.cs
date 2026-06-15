using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhereIsTheTrain.Application.Features.Auth.Commands;
using WhereIsTheTrain.Application.Features.Auth.DTOs;
using WhereIsTheTrain.Application.Features.Auth.Queries;

namespace WhereIsTheTrain.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
    {
        var result = await _mediator.Send(new RegisterCommand(dto.DisplayName, dto.Email, dto.Password));
        return result.IsSuccess ? StatusCode(result.StatusCode, result) : StatusCode(result.StatusCode, result);
    }

    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string token)
    {
        var result = await _mediator.Send(new ConfirmEmailCommand(token));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        var result = await _mediator.Send(new LoginCommand(dto.Email, dto.Password, dto.RememberMe));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto dto)
    {
        var result = await _mediator.Send(new RefreshTokenCommand(dto.AccessToken, dto.RefreshToken));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _mediator.Send(new GetCurrentUserQuery(userId));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }
}
