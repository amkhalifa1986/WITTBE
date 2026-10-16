using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhereIsTheTrain.Application.Features.Admin;
using WhereIsTheTrain.Application.Features.Trips.Queries;

namespace WhereIsTheTrain.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TrainsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TrainsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<IActionResult> Search([FromQuery] string? number, [FromQuery] string? from, [FromQuery] string? to)
    {
        if (!string.IsNullOrWhiteSpace(number))
        {
            var result = await _mediator.Send(new SearchTrainByNumberQuery(number));
            return Ok(result);
        }

        if (!string.IsNullOrWhiteSpace(from) && !string.IsNullOrWhiteSpace(to))
        {
            var result = await _mediator.Send(new SearchTrainByStopsQuery(from, to));
            return Ok(result);
        }

        // If no filter parameters (or partial), return all trains in the database
        var allTrainsResult = await _mediator.Send(new WhereIsTheTrain.Application.Features.Admin.GetAllTrainsQuery());
        if (allTrainsResult.IsSuccess && allTrainsResult.Data != null)
        {
            var trains = allTrainsResult.Data;
            if (!string.IsNullOrWhiteSpace(from))
            {
                trains = trains.Where(t => t.RouteStops.Any(rs => 
                    (rs.StopNameAr != null && rs.StopNameAr.Contains(from, StringComparison.OrdinalIgnoreCase)) ||
                    (rs.StopNameEn != null && rs.StopNameEn.Contains(from, StringComparison.OrdinalIgnoreCase)) ||
                    (rs.StopCode != null && rs.StopCode.Contains(from, StringComparison.OrdinalIgnoreCase))
                )).ToList();
            }
            if (!string.IsNullOrWhiteSpace(to))
            {
                trains = trains.Where(t => t.RouteStops.Any(rs => 
                    (rs.StopNameAr != null && rs.StopNameAr.Contains(to, StringComparison.OrdinalIgnoreCase)) ||
                    (rs.StopNameEn != null && rs.StopNameEn.Contains(to, StringComparison.OrdinalIgnoreCase)) ||
                    (rs.StopCode != null && rs.StopCode.Contains(to, StringComparison.OrdinalIgnoreCase))
                )).ToList();
            }
            return Ok(WhereIsTheTrain.Application.Common.Result<List<WhereIsTheTrain.Application.Features.Trips.DTOs.TrainDto>>.Success(trains));
        }

        return Ok(allTrainsResult);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetTrainDetailsQuery(id));
        return Ok(result);
    }

    [HttpGet("stops")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllStops()
    {
        var result = await _mediator.Send(new WhereIsTheTrain.Application.Features.Admin.GetAllStopsQuery());
        return Ok(result);
    }

    [HttpGet("{id:guid}/trips")]
    public async Task<IActionResult> GetTrips(Guid id)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _mediator.Send(new GetTrainTripsQuery(id, userId));
        return Ok(result);
    }

    [HttpGet("{id:guid}/followers")]
    public async Task<IActionResult> GetFollowers(Guid id)
    {
        var result = await _mediator.Send(new GetTrainFollowersQuery(id));
        return Ok(result);
    }
}

