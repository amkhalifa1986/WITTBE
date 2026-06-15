using MediatR;
using WhereIsTheTrain.Application.Common;
using WhereIsTheTrain.Domain.Entities;
using WhereIsTheTrain.Domain.Interfaces;

namespace WhereIsTheTrain.Application.Features.Admin;

public record GetAdAnalyticsQuery(DateTime? StartDate, DateTime? EndDate, string? TrainNumber = null) : IRequest<Result<AdAnalyticsDto>>;

public class AdAnalyticsDto
{
    public AdAnalyticsSummaryDto Summary { get; set; } = new();
    public List<AdDailyTrendDto> DailyTrend { get; set; } = new();
    public List<AdPageBreakdownDto> PageBreakdown { get; set; } = new();
}

public class AdAnalyticsSummaryDto
{
    public long TotalImpressions { get; set; }
    public long TotalClicks { get; set; }
    public double Ctr { get; set; }
    public long UniqueUsers { get; set; }
}

public class AdDailyTrendDto
{
    public string Date { get; set; } = string.Empty; // yyyy-MM-dd or yyyy-MM-dd HH:00
    public long Impressions { get; set; }
    public long Clicks { get; set; }
}

public class AdPageBreakdownDto
{
    public string ScreenId { get; set; } = string.Empty;
    public long Impressions { get; set; }
    public long Clicks { get; set; }
    public double Ctr { get; set; }
}

public class GetAdAnalyticsQueryHandler : IRequestHandler<GetAdAnalyticsQuery, Result<AdAnalyticsDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAdAnalyticsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    private TimeZoneInfo GetEgyptTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");
        }
    }

    public async Task<Result<AdAnalyticsDto>> Handle(GetAdAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var egyptTimeZone = GetEgyptTimeZone();
        var nowEgypt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);

        // 1. Date Bounds
        var start = request.StartDate ?? nowEgypt.Date.AddDays(-30);
        var end = request.EndDate ?? nowEgypt;

        if (request.EndDate.HasValue)
        {
            end = request.EndDate.Value.Date.AddDays(1).AddTicks(-1);
        }

        bool filterTrain = !string.IsNullOrEmpty(request.TrainNumber) && !request.TrainNumber.Equals("all", StringComparison.OrdinalIgnoreCase);

        // Fetch repositories as queryable with TrainNumber filter applied
        var impressions = await _unitOfWork.Repository<AdImpression>()
            .FindAsync(x => x.Timestamp >= start && x.Timestamp <= end && 
                            (!filterTrain || x.TrainNumber == request.TrainNumber), cancellationToken);

        var clicks = await _unitOfWork.Repository<AdClick>()
            .FindAsync(x => x.Timestamp >= start && x.Timestamp <= end && 
                            (!filterTrain || x.TrainNumber == request.TrainNumber), cancellationToken);

        // 2. Summary stats
        long totalImpressions = impressions.Count;
        long totalClicks = clicks.Count;
        double ctr = totalImpressions > 0 
            ? Math.Round(((double)totalClicks / totalImpressions) * 100, 2)
            : 0.0;

        // Unique users = distinct UserId ?? VisitorId
        long uniqueUsers = impressions
            .Select(x => x.UserId.HasValue ? x.UserId.Value.ToString() : x.VisitorId)
            .Distinct()
            .Count();

        // 3. Daily / Hourly trend
        var dailyTrendList = new List<AdDailyTrendDto>();
        bool isHourly = (end - start) <= TimeSpan.FromDays(1.25);

        if (isHourly)
        {
            for (int h = 0; h < 24; h++)
            {
                var label = $"{start:yyyy-MM-dd} {h:D2}:00";
                long impCount = impressions.Count(x => x.Timestamp.Hour == h);
                long clickCount = clicks.Count(x => x.Timestamp.Hour == h);

                dailyTrendList.Add(new AdDailyTrendDto
                {
                    Date = label,
                    Impressions = impCount,
                    Clicks = clickCount
                });
            }
        }
        else
        {
            var dailyImpressions = impressions
                .GroupBy(x => x.Timestamp.Date)
                .ToDictionary(g => g.Key, g => (long)g.Count());

            var dailyClicks = clicks
                .GroupBy(x => x.Timestamp.Date)
                .ToDictionary(g => g.Key, g => (long)g.Count());

            var dates = dailyImpressions.Keys.Union(dailyClicks.Keys).OrderBy(d => d).ToList();
            if (dates.Count == 0)
            {
                for (var dt = start.Date; dt <= end.Date; dt = dt.AddDays(1))
                {
                    dates.Add(dt);
                }
            }

            dailyTrendList = dates.Select(d => new AdDailyTrendDto
            {
                Date = d.ToString("yyyy-MM-dd"),
                Impressions = dailyImpressions.TryGetValue(d, out var imp) ? imp : 0,
                Clicks = dailyClicks.TryGetValue(d, out var clk) ? clk : 0
            }).ToList();
        }

        // 4. Page breakdowns
        var pageImpressions = impressions
            .GroupBy(x => x.ScreenId)
            .ToDictionary(g => g.Key, g => (long)g.Count());

        var pageClicks = clicks
            .GroupBy(x => x.ScreenId)
            .ToDictionary(g => g.Key, g => (long)g.Count());

        var screenIds = pageImpressions.Keys.Union(pageClicks.Keys).ToList();
        var pageBreakdownList = screenIds.Select(sid => {
            var impCount = pageImpressions.TryGetValue(sid, out var imp) ? imp : 0;
            var clickCount = pageClicks.TryGetValue(sid, out var clk) ? clk : 0;
            return new AdPageBreakdownDto
            {
                ScreenId = sid,
                Impressions = impCount,
                Clicks = clickCount,
                Ctr = impCount > 0 ? Math.Round(((double)clickCount / impCount) * 100, 2) : 0.0
            };
        }).ToList();

        var dto = new AdAnalyticsDto
        {
            Summary = new AdAnalyticsSummaryDto
            {
                TotalImpressions = totalImpressions,
                TotalClicks = totalClicks,
                Ctr = ctr,
                UniqueUsers = uniqueUsers
            },
            DailyTrend = dailyTrendList,
            PageBreakdown = pageBreakdownList
        };

        return Result<AdAnalyticsDto>.Success(dto);
    }
}
