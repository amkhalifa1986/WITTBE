using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhereIsTheTrain.Application.Features.Lookups;

namespace WhereIsTheTrain.API.Controllers;

[ApiController]
public class CrowdLevelsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CrowdLevelsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("api/crowd-levels")]
    public async Task<IActionResult> GetCrowdLevels()
    {
        var result = await _mediator.Send(new GetCrowdLevelsQuery());
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("api/admin/crowd-levels")]
    public async Task<IActionResult> GetAdminCrowdLevels()
    {
        var result = await _mediator.Send(new GetCrowdLevelsQuery());
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("api/admin/crowd-levels")]
    public async Task<IActionResult> CreateCrowdLevel([FromBody] CreateCrowdLevelCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
            return BadRequest(result);
        return StatusCode(201, result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("api/admin/crowd-levels/{id}")]
    public async Task<IActionResult> UpdateCrowdLevel(Guid id, [FromBody] UpdateCrowdLevelCommand command)
    {
        if (id != command.Id)
            return BadRequest(new { Message = "ID mismatch." });
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
            return BadRequest(result);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("api/admin/crowd-levels/{id}")]
    public async Task<IActionResult> DeleteCrowdLevel(Guid id)
    {
        var result = await _mediator.Send(new DeleteCrowdLevelCommand(id));
        if (!result.IsSuccess)
            return BadRequest(result);
        return Ok(result);
    }
}
