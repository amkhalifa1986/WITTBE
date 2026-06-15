using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhereIsTheTrain.Application.Features.AdminAuth.Commands;
using WhereIsTheTrain.Application.Features.AdminAuth.DTOs;
using WhereIsTheTrain.Application.Features.AdminAuth.Queries;
using WhereIsTheTrain.Application.Features.Auth.DTOs;

namespace WhereIsTheTrain.API.Controllers;

[ApiController]
[Route("api/admin/auth")]
public class AdminAuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminAuthController(IMediator mediator) => _mediator = mediator;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AdminLoginRequestDto dto)
    {
        var result = await _mediator.Send(new AdminLoginCommand(dto.Email, dto.Password, dto.RememberMe));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto dto)
    {
        var result = await _mediator.Send(new AdminRefreshTokenCommand(dto.AccessToken, dto.RefreshToken));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentAdmin()
    {
        var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (adminIdClaim == null)
            return Unauthorized();

        var adminId = Guid.Parse(adminIdClaim.Value);
        var result = await _mediator.Send(new GetAdminProfileQuery(adminId));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }
}
