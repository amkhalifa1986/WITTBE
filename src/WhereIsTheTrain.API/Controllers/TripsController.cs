using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WhereIsTheTrain.API.Hubs;
using WhereIsTheTrain.Application.Features.Trips.Commands;
using WhereIsTheTrain.Application.Features.Trips.DTOs;
using WhereIsTheTrain.Application.Features.Trips.Queries;
using WhereIsTheTrain.Domain.Enums;
using WhereIsTheTrain.Application.Features.Admin;

namespace WhereIsTheTrain.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TripsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IHubContext<TripHub> _hubContext;

    public TripsController(IMediator mediator, IHubContext<TripHub> hubContext)
    {
        _mediator = mediator;
        _hubContext = hubContext;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet("today")]
    public async Task<IActionResult> GetTodayTrips()
    {
        var result = await _mediator.Send(new GetTodayTripsQuery(GetUserId()));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTripDetails(Guid id)
    {
        var result = await _mediator.Send(new GetTripDetailsQuery(id, GetUserId()));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpGet("followed")]
    public async Task<IActionResult> GetFollowedTrips()
    {
        var result = await _mediator.Send(new GetFollowedTripsQuery(GetUserId()));
        return Ok(result);
    }

    [HttpPost("{id:guid}/follow")]
    public async Task<IActionResult> FollowTrip(Guid id)
    {
        var result = await _mediator.Send(new FollowTripCommand(id, GetUserId()));
        return result.IsSuccess ? StatusCode(result.StatusCode, result) : StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{id:guid}/follow")]
    public async Task<IActionResult> UnfollowTrip(Guid id)
    {
        var result = await _mediator.Send(new UnfollowTripCommand(id, GetUserId()));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpPut("{id:guid}/my-status")]
    public async Task<IActionResult> MarkPersonalStatus(Guid id, [FromBody] MarkStatusRequest request)
    {
        var result = await _mediator.Send(new MarkPersonalTripStatusCommand(id, GetUserId(), request.Status));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id:guid}/updates")]
    public async Task<IActionResult> GetTripUpdates(Guid id)
    {
        var result = await _mediator.Send(new GetTripDetailsQuery(id, GetUserId()));
        if (!result.IsSuccess) return StatusCode(result.StatusCode, result);
        return Ok(new { IsSuccess = true, Data = result.Data!.RecentUpdates });
    }

    [HttpPost("{id:guid}/updates")]
    public async Task<IActionResult> CreateLiveUpdate(Guid id, [FromBody] CreateLiveUpdateDto dto)
    {
        var result = await _mediator.Send(new CreateLiveUpdateCommand(
            id, GetUserId(), dto.Content, dto.StatusTag, dto.CrowdState, dto.Latitude, dto.Longitude));
        return result.IsSuccess ? StatusCode(result.StatusCode, result) : StatusCode(result.StatusCode, result);
    }

    [HttpGet("notifications")]
    public async Task<IActionResult> GetNotifications()
    {
        var result = await _mediator.Send(new GetNotificationsQuery(GetUserId()));
        return Ok(result);
    }

    [HttpPut("notifications/read")]
    public async Task<IActionResult> MarkAllNotificationsAsRead()
    {
        var result = await _mediator.Send(new MarkNotificationReadCommand(GetUserId(), null, true));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpPut("notifications/{notificationId:guid}/read")]
    public async Task<IActionResult> MarkNotificationAsRead(Guid notificationId)
    {
        var result = await _mediator.Send(new MarkNotificationReadCommand(GetUserId(), notificationId, false));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpPut("{id:guid}/notifications")]
    public async Task<IActionResult> ToggleNotifications(Guid id, [FromBody] NotificationsToggleRequest request)
    {
        var result = await _mediator.Send(new ToggleTripNotificationsCommand(id, GetUserId(), request.Enabled));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpPost("updates/{updateId:guid}/thanks")]
    public async Task<IActionResult> ToggleThanks(Guid updateId)
    {
        var result = await _mediator.Send(new ToggleReportThanksCommand(updateId, GetUserId()));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpPost("updates/{updateId:guid}/removal-request")]
    public async Task<IActionResult> RequestRemoval(Guid updateId)
    {
        var result = await _mediator.Send(new RequestLiveUpdateRemovalCommand(updateId, GetUserId()));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpPost("{id:guid}/telemetry")]
    public async Task<IActionResult> SubmitTelemetry(Guid id, [FromBody] SubmitTelemetryRequest request)
    {
        var result = await _mediator.Send(new SubmitTelemetryCommand(
            id,
            GetUserId(),
            request.Latitude,
            request.Longitude,
            request.Speed
        ));

        if (result.IsSuccess && result.Data != null)
        {
            // Broadcast snapped telemetry to all users tracking this trip
            await _hubContext.Clients.Group($"trip-{id}").SendAsync("ReceiveLocationUpdate", result.Data);
        }

        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id:guid}/tracking")]
    public async Task<IActionResult> GetTripTracking(Guid id)
    {
        var result = await _mediator.Send(new GetTripTrackingQuery(id));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id:guid}/followers")]
    public async Task<IActionResult> GetFollowers(Guid id)
    {
        var result = await _mediator.Send(new GetTripFollowersQuery(id));
        return Ok(result);
    }

    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateTripStatusFollower(Guid id, [FromBody] UpdateTripStatusFollowerRequest request)
    {
        var tripResult = await _mediator.Send(new GetTripDetailsQuery(id, GetUserId()));
        if (!tripResult.IsSuccess || tripResult.Data == null)
            return StatusCode(tripResult.StatusCode, tripResult);

        var trip = tripResult.Data;
        if (!trip.IsFollowedByCurrentUser)
        {
            return StatusCode(403, new { IsSuccess = false, Message = "Only followers can update the status of this trip." });
        }

        if (request.Status != TripStatus.InTransit && request.Status != TripStatus.Arrived)
        {
            return BadRequest(new { IsSuccess = false, Message = "Followers can only transition trip status to InTransit or Arrived." });
        }

        var result = await _mediator.Send(new UpdateTripStatusCommand(id, request.Status, null, null));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }
}

public class MarkStatusRequest
{
    public PersonalTripStatus Status { get; set; }
}

public class NotificationsToggleRequest
{
    public bool Enabled { get; set; }
}

public class SubmitTelemetryRequest
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Speed { get; set; }
}

public class UpdateTripStatusFollowerRequest
{
    public TripStatus Status { get; set; }
}
