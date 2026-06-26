using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhereIsTheTrain.Application.Features.Admin;

namespace WhereIsTheTrain.API.Controllers;

[ApiController]
[Route("api/settings")]
[AllowAnonymous]
public class SettingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SettingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetSettings()
    {
        var result = await _mediator.Send(new GetSystemSettingsQuery());
        if (result.IsSuccess && result.Data != null)
        {
            return Ok(new
            {
                isSuccess = true,
                data = new
                {
                    gpsTrackingEnabled = result.Data.GpsTrackingEnabled
                }
            });
        }

        return Ok(new
        {
            isSuccess = true,
            data = new
            {
                gpsTrackingEnabled = true
            }
        });
    }
}
