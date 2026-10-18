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
                    lostFoundPostAutoPublish = result.Data.LostFoundPostAutoPublish,
                    lostFoundCommentAutoPublish = result.Data.LostFoundCommentAutoPublish,
                    tripLiveUpdateAutoPublish = result.Data.TripLiveUpdateAutoPublish,
                    tripLiveUpdateRemovalAutoApprove = result.Data.TripLiveUpdateRemovalAutoApprove,
                    adsEnabledPages = result.Data.AdsEnabledPages,
                    gpsTrackingEnabled = result.Data.GpsTrackingEnabled
                }
            });
        }

        return Ok(new
        {
            isSuccess = true,
            data = new
            {
                lostFoundPostAutoPublish = true,
                lostFoundCommentAutoPublish = true,
                tripLiveUpdateAutoPublish = true,
                tripLiveUpdateRemovalAutoApprove = false,
                adsEnabledPages = "{}",
                gpsTrackingEnabled = true
            }
        });
    }
}
