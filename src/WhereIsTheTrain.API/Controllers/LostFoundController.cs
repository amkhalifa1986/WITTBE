using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhereIsTheTrain.Application.Features.LostFound;
using WhereIsTheTrain.Domain.Enums;

namespace WhereIsTheTrain.API.Controllers;

[ApiController]
[Route("api/lost-found")]
[Authorize]
public class LostFoundController : ControllerBase
{
    private readonly IMediator _mediator;

    public LostFoundController(IMediator mediator) => _mediator = mediator;

    private Guid GetUserId() => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] LostFoundType? type)
    {
        var result = await _mediator.Send(new GetLostFoundListQuery(type));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDetails(Guid id)
    {
        var result = await _mediator.Send(new GetLostFoundDetailsQuery(id));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLostFoundRequest request)
    {
        var result = await _mediator.Send(new CreateLostFoundPostCommand(
            GetUserId(), request.Title, request.Description, request.ImageUrl,
            request.Type, request.TrainNumber, request.ContactInfo));
        return result.IsSuccess ? StatusCode(result.StatusCode, result) : StatusCode(result.StatusCode, result);
    }

    [HttpPut("{id:guid}/resolve")]
    public async Task<IActionResult> MarkResolved(Guid id)
    {
        var result = await _mediator.Send(new MarkLostFoundResolvedCommand(id, GetUserId()));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    // ==========================================
    // 💬 COMMENTS ENDPOINTS
    // ==========================================

    [HttpPost("{postId:guid}/comments")]
    public async Task<IActionResult> AddComment(Guid postId, [FromBody] AddCommentRequest request)
    {
        var result = await _mediator.Send(new AddLostFoundCommentCommand(postId, GetUserId(), request.Content));
        return result.IsSuccess ? StatusCode(result.StatusCode, result) : StatusCode(result.StatusCode, result);
    }

    [HttpPut("comments/{id:guid}")]
    public async Task<IActionResult> UpdateComment(Guid id, [FromBody] UpdateCommentRequest request)
    {
        var result = await _mediator.Send(new UpdateLostFoundCommentCommand(id, GetUserId(), request.Content));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpDelete("comments/{id:guid}")]
    public async Task<IActionResult> DeleteComment(Guid id)
    {
        var result = await _mediator.Send(new DeleteLostFoundCommentCommand(id, GetUserId()));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }
}

public class CreateLostFoundRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public LostFoundType Type { get; set; }
    public string? TrainNumber { get; set; }
    public string? ContactInfo { get; set; }
}

public class AddCommentRequest
{
    public string Content { get; set; } = string.Empty;
}

public class UpdateCommentRequest
{
    public string Content { get; set; } = string.Empty;
}
