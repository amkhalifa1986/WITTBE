using MediatR;
using WhereIsTheTrain.Application.Common;
using WhereIsTheTrain.Application.Features.Trips.DTOs;
using WhereIsTheTrain.Application.Interfaces;
using WhereIsTheTrain.Domain.Entities;
using WhereIsTheTrain.Domain.Interfaces;

namespace WhereIsTheTrain.Application.Features.Trips.Commands;

// 1. ToggleTripNotificationsCommand
public record ToggleTripNotificationsCommand(Guid TripId, Guid UserId, bool Enabled) : IRequest<Result<bool>>;

public class ToggleTripNotificationsCommandHandler : IRequestHandler<ToggleTripNotificationsCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public ToggleTripNotificationsCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<bool>> Handle(ToggleTripNotificationsCommand request, CancellationToken ct)
    {
        var followerList = await _unitOfWork.Repository<TripFollower>()
            .FindAsync(f => f.TripId == request.TripId && f.UserId == request.UserId, ct);
        var follower = followerList.FirstOrDefault();

        if (follower == null)
            return Result<bool>.Failure("You are not following this trip.", 404);

        follower.NotificationsEnabled = request.Enabled;
        await _unitOfWork.Repository<TripFollower>().UpdateAsync(follower, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<bool>.Success(request.Enabled);
    }
}

// 2. ToggleReportThanksCommand
public record ToggleReportThanksCommand(Guid UpdateId, Guid UserId) : IRequest<Result<ThanksResponseDto>>;

public class ThanksResponseDto
{
    public int ThanksCount { get; set; }
    public bool IsThanked { get; set; }
}

public class ToggleReportThanksCommandHandler : IRequestHandler<ToggleReportThanksCommand, Result<ThanksResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationHubService _hubService;

    public ToggleReportThanksCommandHandler(IUnitOfWork unitOfWork, INotificationHubService hubService)
    {
        _unitOfWork = unitOfWork;
        _hubService = hubService;
    }

    public async Task<Result<ThanksResponseDto>> Handle(ToggleReportThanksCommand request, CancellationToken ct)
    {
        var update = await _unitOfWork.Repository<TripLiveUpdate>().GetByIdAsync(request.UpdateId, ct);
        if (update == null)
            return Result<ThanksResponseDto>.Failure("Live update report not found.", 404);

        var existingThanksList = await _unitOfWork.Repository<TripLiveUpdateThanks>()
            .FindAsync(t => t.TripLiveUpdateId == request.UpdateId && t.UserId == request.UserId, ct);
        var existingThanks = existingThanksList.FirstOrDefault();

        bool isThanked;
        if (existingThanks != null)
        {
            await _unitOfWork.Repository<TripLiveUpdateThanks>().DeleteAsync(existingThanks, ct);
            isThanked = false;
        }
        else
        {
            var thanks = new TripLiveUpdateThanks
            {
                TripLiveUpdateId = request.UpdateId,
                UserId = request.UserId
            };
            await _unitOfWork.Repository<TripLiveUpdateThanks>().AddAsync(thanks, ct);
            isThanked = true;
        }

        await _unitOfWork.SaveChangesAsync(ct);

        // Calculate new count
        var totalThanksList = await _unitOfWork.Repository<TripLiveUpdateThanks>()
            .FindAsync(t => t.TripLiveUpdateId == request.UpdateId, ct);
        int thanksCount = totalThanksList.Count();

        if (isThanked && update.AuthorId.HasValue && update.AuthorId.Value != request.UserId)
        {
            var liker = await _unitOfWork.Users.GetByIdAsync(request.UserId, ct);
            var likerName = liker?.DisplayName ?? "Someone";

            var trip = await _unitOfWork.Trips.GetByIdAsync(update.TripId, ct);
            var train = trip != null ? await _unitOfWork.Trains.GetByIdAsync(trip.TrainId, ct) : null;
            var trainNumber = train?.TrainNumber ?? "Unknown";

            var notification = new Notification
            {
                UserId = update.AuthorId.Value,
                Message = $"{likerName} liked your update on Train {trainNumber}.",
                Link = $"/trip/{update.TripId}",
                IsRead = false
            };

            await _unitOfWork.Repository<Notification>().AddAsync(notification, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            var notificationDto = new NotificationDto
            {
                Id = notification.Id,
                Message = notification.Message,
                Link = notification.Link,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt
            };

            await _hubService.SendNotificationToUserAsync(notification.UserId, notificationDto);
        }

        return Result<ThanksResponseDto>.Success(new ThanksResponseDto
        {
            ThanksCount = thanksCount,
            IsThanked = isThanked
        });
    }
}

// 3. GetNotificationsQuery
public record GetNotificationsQuery(Guid UserId) : IRequest<Result<List<NotificationDto>>>;

public class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, Result<List<NotificationDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetNotificationsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<List<NotificationDto>>> Handle(GetNotificationsQuery request, CancellationToken ct)
    {
        var notifications = await _unitOfWork.Repository<Notification>()
            .FindAsync(n => n.UserId == request.UserId, ct);

        var dtos = notifications
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Message = n.Message,
                Link = n.Link,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            })
            .ToList();

        return Result<List<NotificationDto>>.Success(dtos);
    }
}

// 4. MarkNotificationReadCommand
public record MarkNotificationReadCommand(Guid UserId, Guid? NotificationId, bool MarkAll = false) : IRequest<Result<bool>>;

public class MarkNotificationReadCommandHandler : IRequestHandler<MarkNotificationReadCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public MarkNotificationReadCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<bool>> Handle(MarkNotificationReadCommand request, CancellationToken ct)
    {
        if (request.MarkAll)
        {
            var unread = await _unitOfWork.Repository<Notification>()
                .FindAsync(n => n.UserId == request.UserId && !n.IsRead, ct);

            foreach (var n in unread)
            {
                n.IsRead = true;
                await _unitOfWork.Repository<Notification>().UpdateAsync(n, ct);
            }
        }
        else
        {
            if (!request.NotificationId.HasValue)
                return Result<bool>.Failure("Notification ID is required.", 400);

            var notification = await _unitOfWork.Repository<Notification>().GetByIdAsync(request.NotificationId.Value, ct);
            if (notification == null || notification.UserId != request.UserId)
                return Result<bool>.Failure("Notification not found.", 404);

            notification.IsRead = true;
            await _unitOfWork.Repository<Notification>().UpdateAsync(notification, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }
}
