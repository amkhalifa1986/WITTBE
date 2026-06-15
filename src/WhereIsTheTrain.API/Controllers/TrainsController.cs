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

        return BadRequest(new { IsSuccess = false, Error = "Provide 'number' or both 'from' and 'to' query parameters." });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetTrainDetailsQuery(id));
        return Ok(result);
    }

    [HttpGet("stops")]
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

