using MediatR;
using Microsoft.AspNetCore.Mvc;
using WhereIsTheTrain.Application.Features.Admin;

namespace WhereIsTheTrain.API.Controllers;

[ApiController]
[Route("api/ad-settings")]
public class AdSettingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdSettingsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAdSettings()
    {
        var result = await _mediator.Send(new GetSystemSettingsQuery());
        if (result.IsSuccess && result.Data != null)
        {
            return Ok(new { isSuccess = true, data = result.Data.AdsEnabledPages });
        }
        return Ok(new { isSuccess = true, data = "{}" });
    }
}
