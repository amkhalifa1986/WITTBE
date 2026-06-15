using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace WhereIsTheTrain.API.Hubs;

[Authorize]
public class TripHub : Hub
{
    private readonly ILogger<TripHub> _logger;

    public TripHub(ILogger<TripHub> logger) => _logger = logger;

    public async Task JoinTripGroup(string tripId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"trip-{tripId}");
        _logger.LogInformation("User {UserId} joined trip group {TripId}", Context.UserIdentifier, tripId);
    }

    public async Task LeaveTripGroup(string tripId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"trip-{tripId}");
        _logger.LogInformation("User {UserId} left trip group {TripId}", Context.UserIdentifier, tripId);
    }

    public async Task SendLiveUpdate(string tripId, string content, string? statusTag, double? latitude, double? longitude)
    {
        var update = new
        {
            AuthorId = Context.UserIdentifier,
            Content = content,
            StatusTag = statusTag,
            Latitude = latitude,
            Longitude = longitude,
            CreatedAt = DateTime.UtcNow
        };

        await Clients.OthersInGroup($"trip-{tripId}").SendAsync("ReceiveUpdate", update);
        _logger.LogInformation("Live update sent to trip {TripId} by {UserId}", tripId, Context.UserIdentifier);
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("User {UserId} connected to TripHub", Context.UserIdentifier);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("User {UserId} disconnected from TripHub", Context.UserIdentifier);
        await base.OnDisconnectedAsync(exception);
    }
}
