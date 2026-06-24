using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhereIsTheTrain.Application.Features.Lookups;

namespace WhereIsTheTrain.API.Controllers;

[ApiController]
public class TripStatusesController : ControllerBase
{
    private readonly IMediator _mediator;
    public TripStatusesController(IMediator mediator) => _mediator = mediator;

    [HttpGet("api/trip-statuses")]
    public async Task<IActionResult> GetTripStatuses()
    {
        var result = await _mediator.Send(new GetTripStatusesQuery());
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("api/admin/trip-statuses")]
    public async Task<IActionResult> GetAdminTripStatuses()
    {
        var result = await _mediator.Send(new GetTripStatusesQuery());
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("api/admin/trip-statuses/{id}")]
    public async Task<IActionResult> UpdateTripStatus(Guid id, [FromBody] UpdateTripStatusLookupCommand command)
    {
        if (id != command.Id)
            return BadRequest(new { Message = "ID mismatch." });
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
            return BadRequest(result);
        return Ok(result);
    }
}
