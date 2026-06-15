using Microsoft.AspNetCore.SignalR;
using WhereIsTheTrain.API.Hubs;
using WhereIsTheTrain.Application.Features.Trips.DTOs;
using WhereIsTheTrain.Application.Interfaces;

namespace WhereIsTheTrain.API.Services;

public class NotificationHubService : INotificationHubService
{
    private readonly IHubContext<TripHub> _hubContext;

    public NotificationHubService(IHubContext<TripHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendNotificationToUserAsync(Guid userId, NotificationDto notification)
    {
        await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", notification);
    }

    public async Task SendLiveUpdateToGroupAsync(Guid tripId, LiveUpdateDto liveUpdate)
    {
        await _hubContext.Clients.Group($"trip-{tripId}").SendAsync("ReceiveUpdate", liveUpdate);
    }
}
