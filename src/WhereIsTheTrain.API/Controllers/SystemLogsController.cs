using System;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhereIsTheTrain.Application.Features.SystemLogs;

namespace WhereIsTheTrain.API.Controllers;

[ApiController]
[Route("api/system-logs")]
public class SystemLogsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SystemLogsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateClientLog([FromBody] CreateClientLogRequest request)
    {
        // Resolve optional authenticated user info
        Guid? userId = null;
        string? userEmail = null;

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim != null && Guid.TryParse(userIdClaim, out var parsedId))
        {
            userId = parsedId;
        }

        userEmail = User.FindFirst(ClaimTypes.Email)?.Value;

        var result = await _mediator.Send(new CreateSystemLogCommand(
            request.LogLevel ?? "Error",
            request.Source ?? "Frontend", // Frontend or Mobile
            request.Target ?? string.Empty, // Page or Screen name
            userId,
            userEmail,
            request.Description ?? string.Empty,
            request.ErrorMessage,
            request.StackTrace
        ));

        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAdminLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? logLevel = null,
        [FromQuery] string? source = null,
        [FromQuery] string? search = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null
    )
    {
        var result = await _mediator.Send(new GetSystemLogsQuery(
            page,
            pageSize,
            logLevel,
            source,
            search,
            dateFrom,
            dateTo
        ));

        return Ok(result);
    }

    [HttpDelete("admin/clear")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ClearAllLogs([FromQuery] DateTime? dateFrom = null, [FromQuery] DateTime? dateTo = null)
    {
        var result = await _mediator.Send(new ClearAllSystemLogsCommand(dateFrom, dateTo));
        return StatusCode(result.StatusCode, result);
    }
}

public class CreateClientLogRequest
{
    public string? LogLevel { get; set; }
    public string? Source { get; set; }
    public string? Target { get; set; }
    public string? Description { get; set; }
    public string? ErrorMessage { get; set; }
    public string? StackTrace { get; set; }
}
