using MediatR;
using WhereIsTheTrain.Application.Common;
using WhereIsTheTrain.Application.Features.Trips.DTOs;
using WhereIsTheTrain.Domain.Entities;
using WhereIsTheTrain.Domain.Enums;
using WhereIsTheTrain.Domain.Interfaces;

namespace WhereIsTheTrain.Application.Features.Trips.Commands;

// --- Follow Trip ---
public record FollowTripCommand(Guid TripId, Guid UserId) : IRequest<Result<string>>;

public class FollowTripCommandHandler : IRequestHandler<FollowTripCommand, Result<string>>
{
    private readonly IUnitOfWork _unitOfWork;

    public FollowTripCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<string>> Handle(FollowTripCommand request, CancellationToken ct)
    {
        var trip = await _unitOfWork.Trips.GetByIdAsync(request.TripId, ct);
        if (trip == null)
            return Result<string>.Failure("Trip not found.", 404);

        if (trip.Status == TripStatus.Arrived || trip.Status == TripStatus.Cancelled)
            return Result<string>.Failure("Cannot modify a finished or cancelled trip.", 400);

        var existing = await _unitOfWork.Repository<TripFollower>()
            .FindAsync(f => f.TripId == request.TripId && f.UserId == request.UserId, ct);
        if (existing.Any())
            return Result<string>.Failure("You are already following this trip.", 409);

        var follower = new TripFollower
        {
            UserId = request.UserId,
            TripId = request.TripId,
            PersonalStatus = PersonalTripStatus.Following
        };

        await _unitOfWork.Repository<TripFollower>().AddAsync(follower, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<string>.Success("Trip followed successfully.", 201);
    }
}

// --- Unfollow Trip ---
public record UnfollowTripCommand(Guid TripId, Guid UserId) : IRequest<Result<string>>;

public class UnfollowTripCommandHandler : IRequestHandler<UnfollowTripCommand, Result<string>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UnfollowTripCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<string>> Handle(UnfollowTripCommand request, CancellationToken ct)
    {
        var trip = await _unitOfWork.Trips.GetByIdAsync(request.TripId, ct);
        if (trip == null)
            return Result<string>.Failure("Trip not found.", 404);

        if (trip.Status == TripStatus.Arrived || trip.Status == TripStatus.Cancelled)
            return Result<string>.Failure("Cannot modify a finished or cancelled trip.", 400);

        var followers = await _unitOfWork.Repository<TripFollower>()
            .FindAsync(f => f.TripId == request.TripId && f.UserId == request.UserId, ct);
        var follower = followers.FirstOrDefault();

        if (follower == null)
            return Result<string>.Failure("You are not following this trip.", 404);

        await _unitOfWork.Repository<TripFollower>().DeleteAsync(follower, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<string>.Success("Trip unfollowed successfully.");
    }
}

// --- Mark Personal Trip Status ---
public record MarkPersonalTripStatusCommand(Guid TripId, Guid UserId, PersonalTripStatus Status) : IRequest<Result<string>>;

public class MarkPersonalTripStatusCommandHandler : IRequestHandler<MarkPersonalTripStatusCommand, Result<string>>
{
    private readonly IUnitOfWork _unitOfWork;

    public MarkPersonalTripStatusCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<string>> Handle(MarkPersonalTripStatusCommand request, CancellationToken ct)
    {
        var trip = await _unitOfWork.Trips.GetByIdAsync(request.TripId, ct);
        if (trip == null)
            return Result<string>.Failure("Trip not found.", 404);

        if (trip.Status == TripStatus.Arrived || trip.Status == TripStatus.Cancelled)
            return Result<string>.Failure("Cannot modify a finished or cancelled trip.", 400);

        var followers = await _unitOfWork.Repository<TripFollower>()
            .FindAsync(f => f.TripId == request.TripId && f.UserId == request.UserId, ct);
        var follower = followers.FirstOrDefault();

        if (follower == null)
            return Result<string>.Failure("You are not following this trip.", 404);

        follower.PersonalStatus = request.Status;
        await _unitOfWork.Repository<TripFollower>().UpdateAsync(follower, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<string>.Success($"Trip status updated to {request.Status}.");
    }
}

// --- Create Live Update ---
public record CreateLiveUpdateCommand(
    Guid TripId,
    Guid AuthorId,
    string Content,
    string? StatusTag,
    string? CrowdState,
    double? Latitude,
    double? Longitude
) : IRequest<Result<LiveUpdateDto>>;

public class CreateLiveUpdateCommandHandler : IRequestHandler<CreateLiveUpdateCommand, Result<LiveUpdateDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly TripNotificationHelper _notificationHelper;

    public CreateLiveUpdateCommandHandler(IUnitOfWork unitOfWork, TripNotificationHelper notificationHelper)
    {
        _unitOfWork = unitOfWork;
        _notificationHelper = notificationHelper;
    }

    public async Task<Result<LiveUpdateDto>> Handle(CreateLiveUpdateCommand request, CancellationToken ct)
    {
        var trip = await _unitOfWork.Trips.GetByIdAsync(request.TripId, ct);
        if (trip == null)
            return Result<LiveUpdateDto>.Failure("Trip not found.", 404);

        if (trip.Status == TripStatus.Arrived || trip.Status == TripStatus.Cancelled)
            return Result<LiveUpdateDto>.Failure("Cannot modify a finished or cancelled trip.", 400);

        var author = await _unitOfWork.Users.GetByIdAsync(request.AuthorId, ct);
        if (author == null)
            return Result<LiveUpdateDto>.Failure("User not found.", 404);

        var settings = (await _unitOfWork.Repository<SystemSetting>().GetAllAsync(ct)).FirstOrDefault() ?? new SystemSetting();
        var update = new TripLiveUpdate
        {
            TripId = request.TripId,
            AuthorId = request.AuthorId,
            Content = request.Content,
            StatusTag = request.StatusTag,
            CrowdState = request.CrowdState,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            IsApproved = settings.TripLiveUpdateAutoPublish
        };

        await _unitOfWork.Repository<TripLiveUpdate>().AddAsync(update, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        if (update.IsApproved)
        {
            await _notificationHelper.NotifyFollowersOfNewReportAsync(update.TripId, update, ct);
        }

        return Result<LiveUpdateDto>.Success(new LiveUpdateDto
        {
            Id = update.Id,
            TripId = update.TripId,
            AuthorId = author.Id,
            AuthorName = author.DisplayName,
            AuthorAvatarUrl = author.AvatarUrl,
            Content = update.Content,
            StatusTag = update.StatusTag,
            CrowdState = update.CrowdState,
            Latitude = update.Latitude,
            Longitude = update.Longitude,
            CreatedAt = update.CreatedAt
        }, 201);
    }
}

// --- Request Live Update Removal ---
public record RequestLiveUpdateRemovalCommand(Guid UpdateId, Guid UserId) : IRequest<Result<bool>>;

public class RequestLiveUpdateRemovalCommandHandler : IRequestHandler<RequestLiveUpdateRemovalCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public RequestLiveUpdateRemovalCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<bool>> Handle(RequestLiveUpdateRemovalCommand request, CancellationToken ct)
    {
        var update = await _unitOfWork.Repository<TripLiveUpdate>().GetByIdAsync(request.UpdateId, ct);
        if (update == null)
            return Result<bool>.Failure("Live update not found.", 404);

        if (update.AuthorId != request.UserId)
            return Result<bool>.Failure("You can only request removal of your own updates.", 403);

        // Check system setting: if auto-approve is enabled, delete immediately
        var settings = (await _unitOfWork.Repository<SystemSetting>().GetAllAsync(ct)).FirstOrDefault() ?? new SystemSetting();
        if (settings.TripLiveUpdateRemovalAutoApprove)
        {
            await _unitOfWork.Repository<TripLiveUpdate>().DeleteAsync(update, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return Result<bool>.Success(true);
        }

        // Otherwise queue for admin review
        update.IsRemovalRequested = true;
        await _unitOfWork.Repository<TripLiveUpdate>().UpdateAsync(update, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }
}

