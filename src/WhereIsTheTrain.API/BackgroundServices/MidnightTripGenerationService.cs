using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WhereIsTheTrain.Domain.Entities;
using WhereIsTheTrain.Domain.Enums;
using WhereIsTheTrain.Domain.Interfaces;

namespace WhereIsTheTrain.API.BackgroundServices;

public class MidnightTripGenerationService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MidnightTripGenerationService> _logger;

    public MidnightTripGenerationService(IServiceScopeFactory scopeFactory, ILogger<MidnightTripGenerationService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Midnight Trip Generation Service is starting.");

        // Fallback: Check on startup if today's trips exist.
        try
        {
            await GenerateTripsForDateAsync(WhereIsTheTrain.Domain.Common.DateHelper.GetEgyptToday(), stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to perform startup trip check/generation.");
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = WhereIsTheTrain.Domain.Common.DateHelper.GetEgyptNow();
            var nextMidnight = now.Date.AddDays(1);
            var delay = nextMidnight - now;
            
            _logger.LogInformation("Next midnight trip generation scheduled at {Time} (delay: {Delay}).", nextMidnight, delay);

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }

            // At precisely midnight:
            var retryCount = 0;
            const int maxRetries = 3;
            var success = false;
            var targetDate = WhereIsTheTrain.Domain.Common.DateHelper.GetEgyptToday();

            while (!success && retryCount < maxRetries && !stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Executing midnight trip generation for {Date} (Attempt {Attempt}).", targetDate, retryCount + 1);
                    await GenerateTripsForDateAsync(targetDate, stoppingToken);
                    success = true;
                    _logger.LogInformation("Midnight trip generation for {Date} completed successfully.", targetDate);
                }
                catch (Exception ex)
                {
                    retryCount++;
                    _logger.LogError(ex, "Midnight trip generation failed on attempt {Attempt}.", retryCount);
                    if (retryCount < maxRetries)
                    {
                        _logger.LogInformation("Waiting 5 minutes before retrying midnight trip generation...");
                        try
                        {
                            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                        }
                        catch (TaskCanceledException)
                        {
                            break;
                        }
                    }
                }
            }

            if (!success && !stoppingToken.IsCancellationRequested)
            {
                _logger.LogCritical("Midnight trip generation for {Date} failed after {Max} attempts. Admin intervention required.", targetDate, maxRetries);
            }
        }
    }

    private async Task GenerateTripsForDateAsync(DateOnly date, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var activeTrains = await unitOfWork.Trains.FindAsync(t => t.IsActive, ct);
        var dayOfWeek = (int)date.DayOfWeek; // 0 = Sunday, 1 = Monday, ..., 6 = Saturday

        foreach (var train in activeTrains)
        {
            var existingTrip = await unitOfWork.Trips.GetByTrainAndDateAsync(train.Id, date, ct);
            if (existingTrip == null)
            {
                var trip = new Trip
                {
                    Id = Guid.NewGuid(),
                    TrainId = train.Id,
                    TripDate = date,
                    StatusId = TripStatuses.Scheduled
                };
                
                await unitOfWork.Trips.AddAsync(trip, ct);

                // Fetch follow plans for this train on this day of week
                var followPlans = await unitOfWork.Repository<TrainFollowPlan>()
                    .FindAsync(p => p.TrainId == train.Id && p.DayOfWeek == dayOfWeek, ct);

                foreach (var plan in followPlans)
                {
                    var follower = new TripFollower
                    {
                        Id = Guid.NewGuid(),
                        UserId = plan.UserId,
                        TripId = trip.Id,
                        PersonalStatus = plan.RoleType == TrainFollowRole.Passenger ? PersonalTripStatus.Started : PersonalTripStatus.Following,
                        SourcePlanId = plan.Id
                    };
                    
                    await unitOfWork.Repository<TripFollower>().AddAsync(follower, ct);
                }
            }
        }

        await unitOfWork.SaveChangesAsync(ct);
    }
}
