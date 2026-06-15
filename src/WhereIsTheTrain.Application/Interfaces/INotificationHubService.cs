using WhereIsTheTrain.Application.Features.Trips.DTOs;

namespace WhereIsTheTrain.Application.Interfaces;

public interface INotificationHubService
{
    Task SendNotificationToUserAsync(Guid userId, NotificationDto notification);
    Task SendLiveUpdateToGroupAsync(Guid tripId, LiveUpdateDto liveUpdate);
}
