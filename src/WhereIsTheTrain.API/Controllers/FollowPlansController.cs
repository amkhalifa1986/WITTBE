using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhereIsTheTrain.Application.Features.FollowPlans;
using WhereIsTheTrain.Domain.Enums;

namespace WhereIsTheTrain.API.Controllers;

[ApiController]
[Authorize]
public class FollowPlansController : ControllerBase
{
    private readonly IMediator _mediator;

    public FollowPlansController(IMediator mediator) => _mediator = mediator;

    private Guid GetUserId() => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet("api/trains/{trainId:guid}/follow-plan")]
    public async Task<IActionResult> GetFollowPlan(Guid trainId)
    {
        var result = await _mediator.Send(new GetFollowPlanQuery(trainId, GetUserId()));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpPost("api/trains/{trainId:guid}/follow-plan")]
    public async Task<IActionResult> CreateOrUpdateFollowPlan(Guid trainId, [FromBody] List<CreateOrUpdateFollowPlanRequest> request)
    {
        var result = await _mediator.Send(new CreateOrUpdateFollowPlanCommand(
            trainId,
            GetUserId(),
            request.Select(r => new FollowPlanDayConfig
            {
                DayOfWeek = r.DayOfWeek,
                RoleType = r.RoleType,
                TargetStopId = r.TargetStopId,
                AlertLeadTimeMinutes = r.AlertLeadTimeMinutes
            }).ToList()
        ));
        return result.IsSuccess ? StatusCode(result.StatusCode, result) : StatusCode(result.StatusCode, result);
    }

    [HttpDelete("api/trains/{trainId:guid}/follow-plan")]
    public async Task<IActionResult> DeleteFollowPlan(Guid trainId)
    {
        var result = await _mediator.Send(new DeleteFollowPlanCommand(trainId, GetUserId()));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpGet("api/profile/upcoming-trips")]
    public async Task<IActionResult> GetUpcomingTrips()
    {
        var result = await _mediator.Send(new GetUpcomingFollowedTripsQuery(GetUserId()));
        return Ok(result);
    }

    [HttpGet("api/profile/followed-trains")]
    public async Task<IActionResult> GetFollowedTrains()
    {
        var result = await _mediator.Send(new GetFollowedTrainsQuery(GetUserId()));
        return Ok(result);
    }
}

public class CreateOrUpdateFollowPlanRequest
{
    public int DayOfWeek { get; set; }
    public TrainFollowRole RoleType { get; set; } = TrainFollowRole.Follower;
    public Guid TargetStopId { get; set; }
    public int AlertLeadTimeMinutes { get; set; }
}
