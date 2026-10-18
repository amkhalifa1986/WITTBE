using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WhereIsTheTrain.Application.Common;
using WhereIsTheTrain.Domain.Entities;
using WhereIsTheTrain.Domain.Interfaces;

namespace WhereIsTheTrain.Application.Features.Points;

// --- Award Points ---
public record AwardPointsCommand(Guid UserId, string ActionCode, Guid? TripId = null) : IRequest<bool>;

public class AwardPointsCommandHandler : IRequestHandler<AwardPointsCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    public AwardPointsCommandHandler(IUnitOfWork _unitOfWork) => this._unitOfWork = _unitOfWork;

    public async Task<bool> Handle(AwardPointsCommand request, CancellationToken ct)
    {
        var action = (await _unitOfWork.Repository<PointAction>().FindAsync(a => a.Code == request.ActionCode, ct)).FirstOrDefault();
        if (action == null) return false;

        // Prevent duplicate awards for trip-specific status changes (once per user per action per trip)
        if (request.TripId.HasValue && (request.ActionCode == "StartTrip" || request.ActionCode == "EndTrip"))
        {
            var alreadyAwarded = (await _unitOfWork.Repository<UserPointLog>().FindAsync(
                l => l.UserId == request.UserId && l.PointActionId == action.Id && l.TripId == request.TripId, ct)).Any();
            if (alreadyAwarded) return false;
        }

        var log = new UserPointLog
        {
            UserId = request.UserId,
            PointActionId = action.Id,
            TripId = request.TripId,
            PointsAwarded = action.Points
        };
        await _unitOfWork.Repository<UserPointLog>().AddAsync(log, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return true;
    }
}

public record RevokePointsCommand(Guid UserId, string ActionCode, Guid? TripId = null) : IRequest<bool>;

public class RevokePointsCommandHandler : IRequestHandler<RevokePointsCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    public RevokePointsCommandHandler(IUnitOfWork _unitOfWork) => this._unitOfWork = _unitOfWork;

    public async Task<bool> Handle(RevokePointsCommand request, CancellationToken ct)
    {
        var action = (await _unitOfWork.Repository<PointAction>().FindAsync(a => a.Code == request.ActionCode, ct)).FirstOrDefault();
        if (action == null) return false;

        var logs = await _unitOfWork.Repository<UserPointLog>().FindAsync(
            l => l.UserId == request.UserId && l.PointActionId == action.Id && l.TripId == request.TripId, ct);
        
        var logToRemove = logs.OrderByDescending(l => l.CreatedAt).FirstOrDefault();
        if (logToRemove != null)
        {
            await _unitOfWork.Repository<UserPointLog>().DeleteAsync(logToRemove, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return true;
        }
        return false;
    }
}

// --- Admin Update Point Action ---
public record AdminUpdatePointActionCommand(Guid ActionId, int NewPoints) : IRequest<Result<PointActionDto>>;

public class AdminUpdatePointActionCommandHandler : IRequestHandler<AdminUpdatePointActionCommand, Result<PointActionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    public AdminUpdatePointActionCommandHandler(IUnitOfWork _unitOfWork) => this._unitOfWork = _unitOfWork;

    public async Task<Result<PointActionDto>> Handle(AdminUpdatePointActionCommand request, CancellationToken ct)
    {
        var action = await _unitOfWork.Repository<PointAction>().GetByIdAsync(request.ActionId, ct);
        if (action == null) return Result<PointActionDto>.Failure("Point action not found.", 404);
        action.Points = request.NewPoints;
        await _unitOfWork.Repository<PointAction>().UpdateAsync(action, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result<PointActionDto>.Success(new PointActionDto 
        { 
            Id = action.Id, 
            Code = action.Code, 
            NameEn = action.NameEn, 
            NameAr = action.NameAr, 
            Points = action.Points 
        });
    }
}

// --- Get Point Actions ---
public record GetPointActionsQuery : IRequest<Result<List<PointActionDto>>>;

public class GetPointActionsQueryHandler : IRequestHandler<GetPointActionsQuery, Result<List<PointActionDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetPointActionsQueryHandler(IUnitOfWork _unitOfWork) => this._unitOfWork = _unitOfWork;

    public async Task<Result<List<PointActionDto>>> Handle(GetPointActionsQuery request, CancellationToken ct)
    {
        var actions = await _unitOfWork.Repository<PointAction>().GetAllAsync(ct);
        var dtos = actions.Select(a => new PointActionDto 
        { 
            Id = a.Id, 
            Code = a.Code, 
            NameEn = a.NameEn, 
            NameAr = a.NameAr, 
            Points = a.Points 
        }).ToList();
        return Result<List<PointActionDto>>.Success(dtos);
    }
}

// --- Get Trip Leaderboard ---
public record GetTripLeaderboardQuery(Guid TripId, int Top = 10) : IRequest<Result<List<LeaderboardEntryDto>>>;

public class GetTripLeaderboardQueryHandler : IRequestHandler<GetTripLeaderboardQuery, Result<List<LeaderboardEntryDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetTripLeaderboardQueryHandler(IUnitOfWork _unitOfWork) => this._unitOfWork = _unitOfWork;

    public async Task<Result<List<LeaderboardEntryDto>>> Handle(GetTripLeaderboardQuery request, CancellationToken ct)
    {
        var logs = await _unitOfWork.Repository<UserPointLog>().FindAsync(l => l.TripId == request.TripId, ct);
        var grouped = logs.GroupBy(l => l.UserId)
            .Select(g => new { UserId = g.Key, Total = g.Sum(l => l.PointsAwarded) })
            .OrderByDescending(x => x.Total)
            .Take(request.Top)
            .ToList();

        var result = new List<LeaderboardEntryDto>();
        int rank = 1;
        foreach (var entry in grouped)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(entry.UserId, ct);
            if (user != null)
            {
                result.Add(new LeaderboardEntryDto 
                { 
                    UserId = entry.UserId, 
                    DisplayName = user.DisplayName, 
                    AvatarUrl = user.AvatarUrl, 
                    TotalPoints = entry.Total, 
                    Rank = rank++ 
                });
            }
        }
        return Result<List<LeaderboardEntryDto>>.Success(result);
    }
}

// --- Get Monthly Leaderboard ---
public record GetMonthlyLeaderboardQuery(int Year, int Month, int Top = 10) : IRequest<Result<List<LeaderboardEntryDto>>>;

public class GetMonthlyLeaderboardQueryHandler : IRequestHandler<GetMonthlyLeaderboardQuery, Result<List<LeaderboardEntryDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetMonthlyLeaderboardQueryHandler(IUnitOfWork _unitOfWork) => this._unitOfWork = _unitOfWork;

    public async Task<Result<List<LeaderboardEntryDto>>> Handle(GetMonthlyLeaderboardQuery request, CancellationToken ct)
    {
        var logs = await _unitOfWork.Repository<UserPointLog>().FindAsync(
            l => l.CreatedAt.Year == request.Year && l.CreatedAt.Month == request.Month, ct);

        var grouped = logs.GroupBy(l => l.UserId)
            .Select(g => new { UserId = g.Key, Total = g.Sum(l => l.PointsAwarded) })
            .OrderByDescending(x => x.Total)
            .Take(request.Top)
            .ToList();

        var result = new List<LeaderboardEntryDto>();
        int rank = 1;
        foreach (var entry in grouped)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(entry.UserId, ct);
            if (user != null)
            {
                result.Add(new LeaderboardEntryDto 
                { 
                    UserId = entry.UserId, 
                    DisplayName = user.DisplayName, 
                    AvatarUrl = user.AvatarUrl, 
                    TotalPoints = entry.Total, 
                    Rank = rank++ 
                });
            }
        }
        return Result<List<LeaderboardEntryDto>>.Success(result);
    }
}

// --- Get Yearly Leaderboard ---
public record GetYearlyLeaderboardQuery(int Year, int Top = 10) : IRequest<Result<List<LeaderboardEntryDto>>>;

public class GetYearlyLeaderboardQueryHandler : IRequestHandler<GetYearlyLeaderboardQuery, Result<List<LeaderboardEntryDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetYearlyLeaderboardQueryHandler(IUnitOfWork _unitOfWork) => this._unitOfWork = _unitOfWork;

    public async Task<Result<List<LeaderboardEntryDto>>> Handle(GetYearlyLeaderboardQuery request, CancellationToken ct)
    {
        var logs = await _unitOfWork.Repository<UserPointLog>().FindAsync(l => l.CreatedAt.Year == request.Year, ct);
        var grouped = logs.GroupBy(l => l.UserId)
            .Select(g => new { UserId = g.Key, Total = g.Sum(l => l.PointsAwarded) })
            .OrderByDescending(x => x.Total)
            .Take(request.Top)
            .ToList();

        var result = new List<LeaderboardEntryDto>();
        int rank = 1;
        foreach (var entry in grouped)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(entry.UserId, ct);
            if (user != null)
            {
                result.Add(new LeaderboardEntryDto 
                { 
                    UserId = entry.UserId, 
                    DisplayName = user.DisplayName, 
                    AvatarUrl = user.AvatarUrl, 
                    TotalPoints = entry.Total, 
                    Rank = rank++ 
                });
            }
        }
        return Result<List<LeaderboardEntryDto>>.Success(result);
    }
}

// --- Get User Points Summary ---
public record GetUserPointsQuery(Guid UserId, int? Year = null, int? Month = null) : IRequest<Result<UserPointsSummaryDto>>;

public class GetUserPointsQueryHandler : IRequestHandler<GetUserPointsQuery, Result<UserPointsSummaryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetUserPointsQueryHandler(IUnitOfWork _unitOfWork) => this._unitOfWork = _unitOfWork;

    public async Task<Result<UserPointsSummaryDto>> Handle(GetUserPointsQuery request, CancellationToken ct)
    {
        var logs = (await _unitOfWork.Repository<UserPointLog>().FindAsync(l => l.UserId == request.UserId, ct)).ToList();
        
        if (request.Year.HasValue) logs = logs.Where(l => l.CreatedAt.Year == request.Year.Value).ToList();
        if (request.Month.HasValue) logs = logs.Where(l => l.CreatedAt.Month == request.Month.Value).ToList();

        var allActions = await _unitOfWork.Repository<PointAction>().GetAllAsync(ct);
        var actionDict = allActions.ToDictionary(a => a.Id);

        var logDtos = logs.OrderByDescending(l => l.CreatedAt).Select(l =>
        {
            actionDict.TryGetValue(l.PointActionId, out var action);
            return new UserPointLogDto
            {
                Id = l.Id,
                ActionCode = action?.Code ?? string.Empty,
                ActionNameEn = action?.NameEn ?? string.Empty,
                ActionNameAr = action?.NameAr ?? string.Empty,
                PointsAwarded = l.PointsAwarded,
                TripId = l.TripId,
                CreatedAt = l.CreatedAt
            };
        }).ToList();

        return Result<UserPointsSummaryDto>.Success(new UserPointsSummaryDto
        {
            TotalPoints = logs.Sum(l => l.PointsAwarded),
            Logs = logDtos
        });
    }
}
