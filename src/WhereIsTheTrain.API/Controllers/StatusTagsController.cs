using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhereIsTheTrain.Application.Features.Lookups;

namespace WhereIsTheTrain.API.Controllers;

[ApiController]
public class StatusTagsController : ControllerBase
{
    private readonly IMediator _mediator;

    public StatusTagsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("api/status-tags")]
    public async Task<IActionResult> GetStatusTags()
    {
        var result = await _mediator.Send(new GetStatusTagsQuery());
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("api/admin/status-tags")]
    public async Task<IActionResult> GetAdminStatusTags()
    {
        var result = await _mediator.Send(new GetStatusTagsQuery());
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("api/admin/status-tags")]
    public async Task<IActionResult> CreateStatusTag([FromBody] CreateStatusTagCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
            return BadRequest(result);
        return StatusCode(201, result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("api/admin/status-tags/{id}")]
    public async Task<IActionResult> UpdateStatusTag(Guid id, [FromBody] UpdateStatusTagCommand command)
    {
        if (id != command.Id)
            return BadRequest(new { Message = "ID mismatch." });
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
            return BadRequest(result);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("api/admin/status-tags/{id}")]
    public async Task<IActionResult> DeleteStatusTag(Guid id)
    {
        var result = await _mediator.Send(new DeleteStatusTagCommand(id));
        if (!result.IsSuccess)
            return BadRequest(result);
        return Ok(result);
    }
}
