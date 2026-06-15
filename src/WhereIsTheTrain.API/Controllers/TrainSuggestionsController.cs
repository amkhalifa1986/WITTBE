using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhereIsTheTrain.Application.Features.TrainSuggestions;

namespace WhereIsTheTrain.API.Controllers;

[ApiController]
[Route("api/train-suggestions")]
[Authorize]
public class TrainSuggestionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TrainSuggestionsController(IMediator mediator) => _mediator = mediator;

    private Guid GetUserId() => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpPost]
    public async Task<IActionResult> Suggest([FromBody] SuggestTrainRequest request)
    {
        var result = await _mediator.Send(new SuggestTrainCommand(
            GetUserId(), request.TrainNumber, request.NameAr, request.NameEn, request.DescriptionAr, request.DescriptionEn, request.RouteDescriptionAr, request.RouteDescriptionEn));
        return result.IsSuccess ? StatusCode(result.StatusCode, result) : StatusCode(result.StatusCode, result);
    }

    [HttpGet("mine")]
    public async Task<IActionResult> GetMySuggestions()
    {
        var result = await _mediator.Send(new GetMySuggestionsQuery(GetUserId()));
        return Ok(result);
    }
}

public class SuggestTrainRequest
{
    public string TrainNumber { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }
    public string? RouteDescriptionAr { get; set; }
    public string? RouteDescriptionEn { get; set; }
}
