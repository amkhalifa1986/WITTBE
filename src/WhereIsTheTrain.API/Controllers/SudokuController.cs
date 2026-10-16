using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WhereIsTheTrain.Application.Features.Sudoku;

namespace WhereIsTheTrain.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SudokuController : ControllerBase
{
    private readonly IMediator _mediator;
    public SudokuController(IMediator mediator) => _mediator = mediator;

    [HttpPost("sessions")]
    public async Task<IActionResult> CreateSession([FromBody] CreateSessionRequest request)
    {
        var userId = GetUserId();
        var result = await _mediator.Send(new CreateSudokuSessionCommand(userId, request.Difficulty));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpGet("sessions/active")]
    public async Task<IActionResult> GetActiveSession()
    {
        var userId = GetUserId();
        var result = await _mediator.Send(new GetActiveSudokuSessionQuery(userId));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpPut("sessions/{id:guid}/progress")]
    public async Task<IActionResult> UpdateSessionProgress(Guid id, [FromBody] UpdateSessionProgressRequest request)
    {
        var userId = GetUserId();
        var result = await _mediator.Send(new UpdateSudokuSessionCommand(id, userId, request.CellState, request.ElapsedSeconds));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpPut("sessions/{id:guid}/complete")]
    public async Task<IActionResult> CompleteSession(Guid id, [FromBody] CompleteSessionRequest request)
    {
        var userId = GetUserId();
        var result = await _mediator.Send(new CompleteSudokuSessionCommand(id, userId, request.ElapsedSeconds));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpDelete("sessions/{id:guid}")]
    public async Task<IActionResult> DiscardSession(Guid id)
    {
        var userId = GetUserId();
        var result = await _mediator.Send(new DiscardSudokuSessionCommand(id, userId));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpGet("leaderboard")]
    public async Task<IActionResult> GetLeaderboard([FromQuery] string difficulty)
    {
        var result = await _mediator.Send(new GetSudokuLeaderboardQuery(difficulty));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    [HttpGet("my-best")]
    public async Task<IActionResult> GetMyBest()
    {
        var userId = GetUserId();
        var result = await _mediator.Send(new GetSudokuMyBestQuery(userId));
        return result.IsSuccess ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    private Guid GetUserId()
    {
        return Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
    }
}

public class CreateSessionRequest
{
    public string Difficulty { get; set; } = string.Empty;
}

public class UpdateSessionProgressRequest
{
    public string CellState { get; set; } = string.Empty;
    public int ElapsedSeconds { get; set; }
}

public class CompleteSessionRequest
{
    public int ElapsedSeconds { get; set; }
}
