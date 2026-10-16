using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WhereIsTheTrain.Domain.Common;
using WhereIsTheTrain.Domain.Entities;
using WhereIsTheTrain.Infrastructure.Persistence;
using WhereIsTheTrain.Application.Interfaces;
using WhereIsTheTrain.Application.Features.Trips.DTOs;

namespace WhereIsTheTrain.API.BackgroundServices;

public class TrainMovementTrackingService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TrainMovementTrackingService> _logger;

    public TrainMovementTrackingService(IServiceScopeFactory scopeFactory, ILogger<TrainMovementTrackingService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TrainMovementTrackingService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var hubService = scope.ServiceProvider.GetRequiredService<INotificationHubService>();

                var settings = await context.SystemSettings.AsNoTracking().FirstOrDefaultAsync(stoppingToken) ?? new SystemSetting();
                if (!settings.GpsTrackingEnabled)
                {
                    await TrackTrainMovementsAsync(context, hubService, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in TrainMovementTrackingService cycle.");
            }

            // Wait 30 seconds between checks
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }

        _logger.LogInformation("TrainMovementTrackingService stopped.");
    }

    private async Task TrackTrainMovementsAsync(ApplicationDbContext context, INotificationHubService hubService, CancellationToken ct)
    {
        var egyptToday = DateHelper.GetEgyptToday();
        var egyptNow = DateHelper.GetEgyptNow();
        var currentTimeOfDay = egyptNow.TimeOfDay;

        // Fetch Arrived/Cancelled status IDs from TripStatusLookups
        var endedStatusIds = await context.TripStatusLookups
            .AsNoTracking()
            .Where(s => s.Code == "Arrived" || s.Code == "Cancelled")
            .Select(s => s.Id)
            .ToListAsync(ct);

        // Fetch active trips for today and yesterday (supporting overnight trips)
        var activeTrips = await context.Trips
            .Include(t => t.Train)
                .ThenInclude(tr => tr.RouteStops.OrderBy(rs => rs.StopOrder))
                    .ThenInclude(rs => rs.Stop)
            .Where(t => (t.TripDate == egyptToday || t.TripDate == egyptToday.AddDays(-1)) && !endedStatusIds.Contains(t.StatusId))
            .ToListAsync(ct);

        foreach (var trip in activeTrips)
        {
            var stops = trip.Train.RouteStops.OrderBy(rs => rs.StopOrder).ToList();
            if (!stops.Any()) continue;

            var firstStop = stops.First();
            var tripStartSec = (firstStop.ScheduledDeparture ?? firstStop.ScheduledArrival ?? TimeSpan.Zero).TotalSeconds;

            // Compute current elapsed seconds since the trip started
            double currentSec = currentTimeOfDay.TotalSeconds;
            if (trip.TripDate == egyptToday.AddDays(-1))
            {
                currentSec += 24 * 3600; // Trip started yesterday, so add 24 hours to today's current time
            }
            double elapsedSec = currentSec - tripStartSec;

            foreach (var stop in stops)
            {
                if (!stop.ScheduledArrival.HasValue) continue;

                var arrivalTime = stop.ScheduledArrival.Value;
                double stopArrivalSec = arrivalTime.TotalSeconds;
                if (stopArrivalSec < tripStartSec)
                {
                    stopArrivalSec += 24 * 3600; // Stop is scheduled on the next day
                }

                // Check if the scheduled arrival time has just been reached/passed (within 5-minute window)
                if (elapsedSec >= stopArrivalSec && (elapsedSec - stopArrivalSec) <= 300) // 300 seconds = 5 minutes
                {
                    string link = $"/trip/{trip.Id}?promptStopId={stop.StopId}";

                    // Check if we have already created a notification with this link (to avoid duplicate pushes)
                    bool alreadyNotified = await context.Notifications
                        .AnyAsync(n => n.Link == link, ct);

                    if (!alreadyNotified)
                    {
                        var followers = await context.TripFollowers
                            .Where(f => f.TripId == trip.Id && f.NotificationsEnabled)
                            .ToListAsync(ct);

                        if (followers.Any())
                        {
                            _logger.LogInformation("TrainMovementTrackingService: Pushing arrival notification for Trip {TripId}, Stop {StopId} to {Count} followers.", trip.Id, stop.StopId, followers.Count);

                            var notificationsToInsert = new List<Notification>();
                            foreach (var follower in followers)
                            {
                                var notification = new Notification
                                {
                                    Id = Guid.NewGuid(),
                                    UserId = follower.UserId,
                                    Message = $"Is the trip of the train number {trip.Train.TrainNumber} arrived to stop {stop.Stop.NameEn}?",
                                    Link = link,
                                    IsRead = false,
                                    CreatedAt = DateTime.UtcNow
                                };
                                notificationsToInsert.Add(notification);
                            }

                            await context.Notifications.AddRangeAsync(notificationsToInsert, ct);
                            await context.SaveChangesAsync(ct);

                            // Push real-time notifications via SignalR
                            foreach (var notification in notificationsToInsert)
                            {
                                var notificationDto = new NotificationDto
                                {
                                    Id = notification.Id,
                                    Message = notification.Message,
                                    Link = notification.Link,
                                    IsRead = notification.IsRead,
                                    CreatedAt = notification.CreatedAt
                                };
                                await hubService.SendNotificationToUserAsync(notification.UserId, notificationDto);
                            }
                        }
                    }
                }
            }
        }
    }
}
