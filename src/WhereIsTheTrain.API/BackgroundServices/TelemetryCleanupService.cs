using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WhereIsTheTrain.Infrastructure.Persistence;

namespace WhereIsTheTrain.API.BackgroundServices;

/// <summary>
/// Background service that runs once daily to delete TripTelemetry records older
/// than 48 hours. Without cleanup, the telemetry table grows by ~864,000 rows/day
/// with 100 active GPS users — causing full table scans to degrade over time.
/// </summary>
public class TelemetryCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TelemetryCleanupService> _logger;

    // Retention window: delete telemetry older than 48 hours
    private const int RetentionHours = 48;

    // Batch size to avoid long-running DELETE statements locking the table
    private const int BatchSize = 5000;

    public TelemetryCleanupService(IServiceScopeFactory scopeFactory, ILogger<TelemetryCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TelemetryCleanupService started. Retention window: {Hours}h.", RetentionHours);

        while (!stoppingToken.IsCancellationRequested)
        {
            // Calculate delay until next 2:00 AM UTC
            var delay = CalculateDelayUntil2Am();
            _logger.LogInformation(
                "TelemetryCleanupService: next cleanup in {Hours:F1}h at ~02:00 UTC.",
                delay.TotalHours);

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            await RunCleanupAsync(stoppingToken);
        }

        _logger.LogInformation("TelemetryCleanupService stopped.");
    }

    private async Task RunCleanupAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var cutoff = DateTime.UtcNow.AddHours(-RetentionHours);
            var totalDeleted = 0;

            // Delete in batches to avoid table-level locks under load
            int deleted;
            do
            {
                deleted = await context.Database.ExecuteSqlRawAsync(
                    $"DELETE FROM TripTelemetry WHERE Timestamp < {{0}} LIMIT {BatchSize}",
                    new object[] { cutoff },
                    ct);
                totalDeleted += deleted;

                if (deleted > 0)
                {
                    // Small pause between batches to allow other queries to proceed
                    await Task.Delay(200, ct);
                }
            }
            while (deleted == BatchSize && !ct.IsCancellationRequested);

            _logger.LogInformation(
                "TelemetryCleanupService: deleted {Count} telemetry records older than {Hours}h (cutoff: {Cutoff:u}).",
                totalDeleted, RetentionHours, cutoff);
        }
        catch (OperationCanceledException)
        {
            // Shutdown — expected
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TelemetryCleanupService: error during cleanup run.");
        }
    }

    /// <summary>
    /// Calculates the delay until the next 02:00 UTC occurrence.
    /// If it is already past 02:00, schedules for tomorrow's 02:00.
    /// </summary>
    private static TimeSpan CalculateDelayUntil2Am()
    {
        var now = DateTime.UtcNow;
        var next2Am = new DateTime(now.Year, now.Month, now.Day, 2, 0, 0, DateTimeKind.Utc);

        if (now >= next2Am)
        {
            next2Am = next2Am.AddDays(1);
        }

        return next2Am - now;
    }
}
