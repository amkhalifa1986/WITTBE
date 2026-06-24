using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhereIsTheTrain.Application.Features.Lookups;

namespace WhereIsTheTrain.API.Controllers;

[ApiController]
public class GenderLookupsController : ControllerBase
{
    private readonly IMediator _mediator;

    public GenderLookupsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("api/genders")]
    public async Task<IActionResult> GetGenders()
    {
        var result = await _mediator.Send(new GetGendersQuery());
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("api/admin/genders")]
    public async Task<IActionResult> GetAdminGenders()
    {
        var result = await _mediator.Send(new GetGendersQuery());
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("api/admin/genders")]
    public async Task<IActionResult> CreateGender([FromBody] CreateGenderCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
            return BadRequest(result);
        return StatusCode(201, result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("api/admin/genders/{id}")]
    public async Task<IActionResult> UpdateGender(Guid id, [FromBody] UpdateGenderCommand command)
    {
        if (id != command.Id)
            return BadRequest(new { Message = "ID mismatch." });
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
            return BadRequest(result);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("api/admin/genders/{id}")]
    public async Task<IActionResult> DeleteGender(Guid id)
    {
        var result = await _mediator.Send(new DeleteGenderCommand(id));
        if (!result.IsSuccess)
            return BadRequest(result);
        return Ok(result);
    }
}
