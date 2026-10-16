using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WhereIsTheTrain.Application.Common;
using WhereIsTheTrain.Domain.Entities;
using WhereIsTheTrain.Domain.Interfaces;

namespace WhereIsTheTrain.Application.Features.Sudoku;

// --- Commands ---

public record CreateSudokuSessionCommand(Guid UserId, string Difficulty) : IRequest<Result<SudokuSessionDto>>;

public class CreateSudokuSessionCommandHandler : IRequestHandler<CreateSudokuSessionCommand, Result<SudokuSessionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    public CreateSudokuSessionCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<SudokuSessionDto>> Handle(CreateSudokuSessionCommand request, CancellationToken ct)
    {
        // Check for existing active session
        var activeSession = (await _unitOfWork.Repository<SudokuSession>().FindAsync(
            s => s.UserId == request.UserId && !s.IsCompleted && !s.IsDiscarded, ct)).FirstOrDefault();

        if (activeSession != null)
        {
            return Result<SudokuSessionDto>.Failure("An active Sudoku session already exists.", 400);
        }

        // Validate difficulty
        var diff = request.Difficulty;
        if (diff != "Easy" && diff != "Medium" && diff != "Hard")
        {
            return Result<SudokuSessionDto>.Failure("Invalid difficulty level.", 400);
        }

        var session = new SudokuSession
        {
            UserId = request.UserId,
            Difficulty = diff,
            PuzzleIndex = Random.Shared.Next(0, 20),
            CellState = new string('0', 81), // Starts as empty representation
            ElapsedSeconds = 0,
            IsCompleted = false,
            IsDiscarded = false
        };

        await _unitOfWork.Repository<SudokuSession>().AddAsync(session, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<SudokuSessionDto>.Success(new SudokuSessionDto
        {
            Id = session.Id,
            Difficulty = session.Difficulty,
            PuzzleIndex = session.PuzzleIndex,
            CellState = session.CellState,
            ElapsedSeconds = session.ElapsedSeconds
        });
    }
}

public record UpdateSudokuSessionCommand(Guid SessionId, Guid UserId, string CellState, int ElapsedSeconds) : IRequest<Result<bool>>;

public class UpdateSudokuSessionCommandHandler : IRequestHandler<UpdateSudokuSessionCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    public UpdateSudokuSessionCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<bool>> Handle(UpdateSudokuSessionCommand request, CancellationToken ct)
    {
        var session = await _unitOfWork.Repository<SudokuSession>().GetByIdAsync(request.SessionId, ct);
        if (session == null) return Result<bool>.Failure("Session not found.", 404);
        if (session.UserId != request.UserId) return Result<bool>.Failure("Unauthorized.", 403);
        if (session.IsCompleted || session.IsDiscarded) return Result<bool>.Failure("Session is not active.", 400);

        if (string.IsNullOrEmpty(request.CellState) || request.CellState.Length != 81)
        {
            return Result<bool>.Failure("Invalid cell state length.", 400);
        }

        session.CellState = request.CellState;
        session.ElapsedSeconds = request.ElapsedSeconds;

        await _unitOfWork.Repository<SudokuSession>().UpdateAsync(session, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }
}

public record CompleteSudokuSessionCommand(Guid SessionId, Guid UserId, int ElapsedSeconds) : IRequest<Result<bool>>;

public class CompleteSudokuSessionCommandHandler : IRequestHandler<CompleteSudokuSessionCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    public CompleteSudokuSessionCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<bool>> Handle(CompleteSudokuSessionCommand request, CancellationToken ct)
    {
        var session = await _unitOfWork.Repository<SudokuSession>().GetByIdAsync(request.SessionId, ct);
        if (session == null) return Result<bool>.Failure("Session not found.", 404);
        if (session.UserId != request.UserId) return Result<bool>.Failure("Unauthorized.", 403);
        if (session.IsDiscarded) return Result<bool>.Failure("Session has been discarded.", 400);

        if (session.IsCompleted) return Result<bool>.Success(true); // Already completed

        session.IsCompleted = true;
        session.ElapsedSeconds = request.ElapsedSeconds;
        await _unitOfWork.Repository<SudokuSession>().UpdateAsync(session, ct);

        // Check if this is a personal best
        var previousBest = (await _unitOfWork.Repository<SudokuScore>().FindAsync(
            s => s.UserId == request.UserId && s.Difficulty == session.Difficulty && s.IsPersonalBest, ct)).FirstOrDefault();

        bool isNewBest = false;
        if (previousBest == null)
        {
            isNewBest = true;
        }
        else if (request.ElapsedSeconds < previousBest.TimeSeconds)
        {
            isNewBest = true;
            previousBest.IsPersonalBest = false;
            await _unitOfWork.Repository<SudokuScore>().UpdateAsync(previousBest, ct);
        }

        var score = new SudokuScore
        {
            UserId = request.UserId,
            Difficulty = session.Difficulty,
            TimeSeconds = request.ElapsedSeconds,
            IsPersonalBest = isNewBest
        };

        await _unitOfWork.Repository<SudokuScore>().AddAsync(score, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }
}

public record DiscardSudokuSessionCommand(Guid SessionId, Guid UserId) : IRequest<Result<bool>>;

public class DiscardSudokuSessionCommandHandler : IRequestHandler<DiscardSudokuSessionCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    public DiscardSudokuSessionCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<bool>> Handle(DiscardSudokuSessionCommand request, CancellationToken ct)
    {
        var session = await _unitOfWork.Repository<SudokuSession>().GetByIdAsync(request.SessionId, ct);
        if (session == null) return Result<bool>.Failure("Session not found.", 404);
        if (session.UserId != request.UserId) return Result<bool>.Failure("Unauthorized.", 403);

        session.IsDiscarded = true;
        await _unitOfWork.Repository<SudokuSession>().UpdateAsync(session, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }
}

// --- Queries ---

public record GetActiveSudokuSessionQuery(Guid UserId) : IRequest<Result<SudokuSessionDto?>>;

public class GetActiveSudokuSessionQueryHandler : IRequestHandler<GetActiveSudokuSessionQuery, Result<SudokuSessionDto?>>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetActiveSudokuSessionQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<SudokuSessionDto?>> Handle(GetActiveSudokuSessionQuery request, CancellationToken ct)
    {
        var session = (await _unitOfWork.Repository<SudokuSession>().FindAsync(
            s => s.UserId == request.UserId && !s.IsCompleted && !s.IsDiscarded, ct)).FirstOrDefault();

        if (session == null) return Result<SudokuSessionDto?>.Success(null);

        return Result<SudokuSessionDto?>.Success(new SudokuSessionDto
        {
            Id = session.Id,
            Difficulty = session.Difficulty,
            PuzzleIndex = session.PuzzleIndex,
            CellState = session.CellState,
            ElapsedSeconds = session.ElapsedSeconds
        });
    }
}

public record GetSudokuLeaderboardQuery(string Difficulty) : IRequest<Result<List<SudokuLeaderboardEntryDto>>>;

public class GetSudokuLeaderboardQueryHandler : IRequestHandler<GetSudokuLeaderboardQuery, Result<List<SudokuLeaderboardEntryDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetSudokuLeaderboardQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<List<SudokuLeaderboardEntryDto>>> Handle(GetSudokuLeaderboardQuery request, CancellationToken ct)
    {
        var scores = (await _unitOfWork.Repository<SudokuScore>().FindAsync(
            s => s.Difficulty == request.Difficulty && s.IsPersonalBest, ct))
            .OrderBy(s => s.TimeSeconds)
            .Take(20)
            .ToList();

        var userIds = scores.Select(s => s.UserId).Distinct().ToList();
        var users = (await _unitOfWork.Users.FindAsync(u => userIds.Contains(u.Id), ct))
            .ToDictionary(u => u.Id);

        var result = new List<SudokuLeaderboardEntryDto>();
        int rank = 1;
        foreach (var score in scores)
        {
            users.TryGetValue(score.UserId, out var user);
            result.Add(new SudokuLeaderboardEntryDto
            {
                Rank = rank++,
                UserName = user?.DisplayName ?? "Unknown",
                AvatarUrl = user?.AvatarUrl,
                TimeSeconds = score.TimeSeconds,
                CreatedAt = score.CreatedAt
            });
        }

        return Result<List<SudokuLeaderboardEntryDto>>.Success(result);
    }
}

public record GetSudokuMyBestQuery(Guid UserId) : IRequest<Result<List<SudokuMyBestDto>>>;

public class GetSudokuMyBestQueryHandler : IRequestHandler<GetSudokuMyBestQuery, Result<List<SudokuMyBestDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetSudokuMyBestQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<List<SudokuMyBestDto>>> Handle(GetSudokuMyBestQuery request, CancellationToken ct)
    {
        var scores = await _unitOfWork.Repository<SudokuScore>().FindAsync(
            s => s.UserId == request.UserId && s.IsPersonalBest, ct);

        var dtos = scores.Select(s => new SudokuMyBestDto
        {
            Difficulty = s.Difficulty,
            BestTimeSeconds = s.TimeSeconds
        }).ToList();

        return Result<List<SudokuMyBestDto>>.Success(dtos);
    }
}
