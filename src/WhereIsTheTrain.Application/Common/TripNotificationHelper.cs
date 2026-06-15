using WhereIsTheTrain.Application.Features.Trips.DTOs;
using WhereIsTheTrain.Application.Interfaces;
using WhereIsTheTrain.Domain.Entities;
using WhereIsTheTrain.Domain.Interfaces;

namespace WhereIsTheTrain.Application.Common;

public class TripNotificationHelper
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationHubService _hubService;

    public TripNotificationHelper(IUnitOfWork unitOfWork, INotificationHubService hubService)
    {
        _unitOfWork = unitOfWork;
        _hubService = hubService;
    }

    public async Task NotifyFollowersOfTripStatusAsync(Guid tripId, string oldStatus, string newStatus, CancellationToken ct = default)
    {
        var trip = await _unitOfWork.Trips.GetByIdAsync(tripId, ct);
        if (trip == null) return;

        var train = await _unitOfWork.Trains.GetByIdAsync(trip.TrainId, ct);
        var trainNumber = train?.TrainNumber ?? "Unknown";

        // 1. Create a system-generated TripLiveUpdate report
        var systemReport = new TripLiveUpdate
        {
            TripId = tripId,
            AuthorId = null, // system report
            Content = $"Trip status changed from {oldStatus} to {newStatus}.",
            StatusTag = newStatus,
            IsApproved = true
        };

        await _unitOfWork.Repository<TripLiveUpdate>().AddAsync(systemReport, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // 2. Resolve all followers with notifications toggled ON
        var followers = await _unitOfWork.Repository<TripFollower>()
            .FindAsync(f => f.TripId == tripId && f.NotificationsEnabled, ct);

        // 3. Create Notification records
        var notifications = new List<Notification>();
        foreach (var follower in followers)
        {
            var notification = new Notification
            {
                UserId = follower.UserId,
                Message = $"Trip {trainNumber}: Status changed from {oldStatus} to {newStatus}.",
                Link = $"/trip/{tripId}",
                IsRead = false
            };
            await _unitOfWork.Repository<Notification>().AddAsync(notification, ct);
            notifications.Add(notification);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        // 4. Broadcast live update to group
        var liveUpdateDto = new LiveUpdateDto
        {
            Id = systemReport.Id,
            TripId = tripId,
            AuthorId = null,
            AuthorName = "System",
            AuthorAvatarUrl = null,
            Content = systemReport.Content,
            StatusTag = systemReport.StatusTag,
            CrowdState = null,
            Latitude = null,
            Longitude = null,
            CreatedAt = systemReport.CreatedAt,
            ThanksCount = 0,
            IsThankedByCurrentUser = false
        };

        await _hubService.SendLiveUpdateToGroupAsync(tripId, liveUpdateDto);

        // 5. Push real-time notifications to followers via SignalR
        foreach (var n in notifications)
        {
            var notificationDto = new NotificationDto
            {
                Id = n.Id,
                Message = n.Message,
                Link = n.Link,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            };
            await _hubService.SendNotificationToUserAsync(n.UserId, notificationDto);
        }
    }

    public async Task NotifyFollowersOfNewReportAsync(Guid tripId, TripLiveUpdate report, CancellationToken ct = default)
    {
        var trip = await _unitOfWork.Trips.GetByIdAsync(tripId, ct);
        if (trip == null) return;

        var train = await _unitOfWork.Trains.GetByIdAsync(trip.TrainId, ct);
        var trainNumber = train?.TrainNumber ?? "Unknown";

        // Find followers (excluding the report author if it's a passenger update)
        var followers = await _unitOfWork.Repository<TripFollower>()
            .FindAsync(f => f.TripId == tripId && f.NotificationsEnabled && f.UserId != report.AuthorId, ct);

        // Create Notifications
        var notifications = new List<Notification>();
        foreach (var follower in followers)
        {
            var notification = new Notification
            {
                UserId = follower.UserId,
                Message = $"Trip {trainNumber}: New passenger report posted.",
                Link = $"/trip/{tripId}",
                IsRead = false
            };
            await _unitOfWork.Repository<Notification>().AddAsync(notification, ct);
            notifications.Add(notification);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        // Resolve author name
        string authorName = "System";
        string? authorAvatarUrl = null;
        if (report.AuthorId.HasValue)
        {
            var author = await _unitOfWork.Users.GetByIdAsync(report.AuthorId.Value, ct);
            if (author != null)
            {
                authorName = author.DisplayName;
                authorAvatarUrl = author.AvatarUrl;
            }
        }

        // Broadcast report to the group
        var liveUpdateDto = new LiveUpdateDto
        {
            Id = report.Id,
            TripId = tripId,
            AuthorId = report.AuthorId,
            AuthorName = authorName,
            AuthorAvatarUrl = authorAvatarUrl,
            Content = report.Content,
            StatusTag = report.StatusTag,
            CrowdState = report.CrowdState,
            Latitude = report.Latitude,
            Longitude = report.Longitude,
            CreatedAt = report.CreatedAt,
            ThanksCount = 0,
            IsThankedByCurrentUser = false
        };

        await _hubService.SendLiveUpdateToGroupAsync(tripId, liveUpdateDto);

        // Send real-time notifications to followers
        foreach (var n in notifications)
        {
            var notificationDto = new NotificationDto
            {
                Id = n.Id,
                Message = n.Message,
                Link = n.Link,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            };
            await _hubService.SendNotificationToUserAsync(n.UserId, notificationDto);
        }
    }
}
