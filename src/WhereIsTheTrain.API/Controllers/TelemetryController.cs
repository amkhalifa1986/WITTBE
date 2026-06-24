using System;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WhereIsTheTrain.API.Hubs;
using WhereIsTheTrain.Application.Features.Trips.Commands;
using WhereIsTheTrain.Application.Features.Trips.Queries;
using WhereIsTheTrain.Domain.Interfaces;
using WhereIsTheTrain.Domain.Entities;

namespace WhereIsTheTrain.API.Controllers;

[ApiController]
[Authorize]
public class TelemetryController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IHubContext<TripHub> _hubContext;
    private readonly IUnitOfWork _unitOfWork;

    public TelemetryController(IMediator mediator, IHubContext<TripHub> hubContext, IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _hubContext = hubContext;
        _unitOfWork = unitOfWork;
    }

    [HttpGet("api/admin/trains/update-903-time")]
    [AllowAnonymous]
    public async Task<IActionResult> Update903Time()
    {
        var trainId = Guid.Parse("20000000-0000-0000-0000-000000000002");
        var routeStops = await _unitOfWork.Repository<TrainRouteStop>().FindAsync(rs => rs.TrainId == trainId);
        
        foreach (var rs in routeStops)
        {
            if (rs.StopOrder == 1)
            {
                rs.ScheduledDeparture = new TimeSpan(19, 0, 0); // 7:00 pm
                rs.ScheduledArrival = null;
            }
            else if (rs.StopOrder == 2)
            {
                rs.ScheduledArrival = new TimeSpan(20, 15, 0);
                rs.ScheduledDeparture = new TimeSpan(20, 18, 0);
            }
            else if (rs.StopOrder == 3)
            {
                rs.ScheduledArrival = new TimeSpan(20, 55, 0);
                rs.ScheduledDeparture = new TimeSpan(20, 58, 0);
            }
            else if (rs.StopOrder == 4)
            {
                rs.ScheduledArrival = new TimeSpan(21, 30, 0);
                rs.ScheduledDeparture = null;
            }
            await _unitOfWork.Repository<TrainRouteStop>().UpdateAsync(rs);
        }

        // Reset trip status and clear old telemetry history
        var tripId = Guid.Parse("f03191cf-1a6f-4937-a84b-323eb1260e89");
        var trip = await _unitOfWork.Repository<Trip>().GetByIdAsync(tripId);
        if (trip != null)
        {
            trip.StatusId = WhereIsTheTrain.Domain.Entities.TripStatuses.Scheduled;
            await _unitOfWork.Repository<Trip>().UpdateAsync(trip);
        }

        var telemetries = await _unitOfWork.Repository<TripTelemetry>().FindAsync(t => t.TripId == tripId);
        foreach (var t in telemetries)
        {
            await _unitOfWork.Repository<TripTelemetry>().DeleteAsync(t);
        }

        await _unitOfWork.SaveChangesAsync();
        return Ok(new { isSuccess = true, data = "Train 903 stop schedule updated successfully to start at 7:00 PM!" });
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

}
