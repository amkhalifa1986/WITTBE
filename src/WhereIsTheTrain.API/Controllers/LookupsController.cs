using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhereIsTheTrain.Application.Features.Lookups;

namespace WhereIsTheTrain.API.Controllers;

[ApiController]
[Route("api/lookups")]
[Authorize]
public class LookupsController : ControllerBase
{
    private readonly IMediator _mediator;

    public LookupsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("cities")]
    public async Task<IActionResult> GetCities()
    {
        var result = await _mediator.Send(new GetCitiesQuery());
        return Ok(result);
    }

    [HttpGet("governorates")]
    public async Task<IActionResult> GetGovernorates()
    {
        var result = await _mediator.Send(new GetGovernoratesQuery());
        return Ok(result);
    }
}
