using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhereIsTheTrain.Application.Features.TrainSuggestions;

namespace WhereIsTheTrain.API.Controllers;

[ApiController]
[Route("api/stop-suggestions")]
[Authorize]
public class StopSuggestionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public StopSuggestionsController(IMediator mediator) => _mediator = mediator;

    private Guid GetUserId() => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpPost]
    public async Task<IActionResult> Suggest([FromBody] SuggestStopRequest request)
    {
        var result = await _mediator.Send(new SuggestStopCommand(
            GetUserId(), 
            request.Code, 
            request.NameAr, 
            request.NameEn, 
            request.CityId, 
            request.NewCityNameAr, 
            request.NewCityNameEn, 
            request.NewCityGovernorateId,
            request.Latitude, 
            request.Longitude, 
            request.DescriptionAr, 
            request.DescriptionEn
        ));
        
        return result.IsSuccess ? StatusCode(result.StatusCode, result) : StatusCode(result.StatusCode, result);
    }

    [HttpGet("mine")]
    public async Task<IActionResult> GetMySuggestions()
    {
        var result = await _mediator.Send(new GetMyStopSuggestionsQuery(GetUserId()));
        return Ok(result);
    }
}

public class SuggestStopRequest
{
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    
    public Guid? CityId { get; set; }
    
    public string? NewCityNameAr { get; set; }
    public string? NewCityNameEn { get; set; }
    public Guid? NewCityGovernorateId { get; set; }
    
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }
}
