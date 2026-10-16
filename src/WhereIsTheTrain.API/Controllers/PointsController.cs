using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhereIsTheTrain.Application.Features.Points;

namespace WhereIsTheTrain.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PointsController : ControllerBase
{
    private readonly IMediator _mediator;
    public PointsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("actions")]
    public async Task<IActionResult> GetPointActions()
    {
        var result = await _mediator.Send(new GetPointActionsQuery());
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpGet("leaderboard/trip/{tripId:guid}")]
    public async Task<IActionResult> GetTripLeaderboard(Guid tripId, [FromQuery] int top = 50)
    {
        var result = await _mediator.Send(new GetTripLeaderboardQuery(tripId, top));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpGet("leaderboard/monthly")]
    public async Task<IActionResult> GetMonthlyLeaderboard([FromQuery] int? year = null, [FromQuery] int? month = null)
    {
        var y = year ?? DateTime.UtcNow.Year;
        var m = month ?? DateTime.UtcNow.Month;
        var result = await _mediator.Send(new GetMonthlyLeaderboardQuery(y, m, 10));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpGet("leaderboard/yearly")]
    public async Task<IActionResult> GetYearlyLeaderboard([FromQuery] int? year = null)
    {
        var y = year ?? DateTime.UtcNow.Year;
        var result = await _mediator.Send(new GetYearlyLeaderboardQuery(y, 10));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpGet("my-points")]
    public async Task<IActionResult> GetMyPoints([FromQuery] int? year = null, [FromQuery] int? month = null)
    {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var result = await _mediator.Send(new GetUserPointsQuery(userId, year, month));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }
}
