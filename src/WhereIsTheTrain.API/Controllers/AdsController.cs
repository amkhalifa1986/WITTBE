using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WhereIsTheTrain.Application.Features.Ads;

namespace WhereIsTheTrain.API.Controllers;

[ApiController]
[Route("api/ads")]
public class AdsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("impression")]
    [AllowAnonymous]
    public async Task<IActionResult> LogImpression([FromBody] LogAdEventRequest request)
    {
        Guid? userId = null;
        var nameIdentifierClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (nameIdentifierClaim != null && Guid.TryParse(nameIdentifierClaim.Value, out var parsedUserId))
        {
            userId = parsedUserId;
        }

        var result = await _mediator.Send(new LogAdImpressionCommand(request.ScreenId, request.VisitorId, userId, request.TrainNumber));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("click")]
    [AllowAnonymous]
    public async Task<IActionResult> LogClick([FromBody] LogAdEventRequest request)
    {
        Guid? userId = null;
        var nameIdentifierClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (nameIdentifierClaim != null && Guid.TryParse(nameIdentifierClaim.Value, out var parsedUserId))
        {
            userId = parsedUserId;
        }

        var result = await _mediator.Send(new LogAdClickCommand(request.ScreenId, request.VisitorId, userId, request.TrainNumber));
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}

public class LogAdEventRequest
{
    public string ScreenId { get; set; } = string.Empty;
    public string VisitorId { get; set; } = string.Empty;
    public string? TrainNumber { get; set; }
}
