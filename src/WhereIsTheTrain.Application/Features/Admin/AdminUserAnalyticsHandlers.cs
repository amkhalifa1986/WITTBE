using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using WhereIsTheTrain.Application.Common;
using WhereIsTheTrain.Domain.Entities;
using WhereIsTheTrain.Domain.Enums;
using WhereIsTheTrain.Domain.Interfaces;

namespace WhereIsTheTrain.Application.Features.Admin;

public class AnalyticsDataPointDto
{
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
}

public record GetRegistrationAnalyticsQuery(string Timeframe, string GenderFilter) : IRequest<Result<List<AnalyticsDataPointDto>>>;

public class GetRegistrationAnalyticsQueryHandler : IRequestHandler<GetRegistrationAnalyticsQuery, Result<List<AnalyticsDataPointDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetRegistrationAnalyticsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<List<AnalyticsDataPointDto>>> Handle(GetRegistrationAnalyticsQuery request, CancellationToken ct)
    {
        await Task.CompletedTask;
        var query = _unitOfWork.Repository<User>().AsQueryable();

        if (request.GenderFilter != "All" && Guid.TryParse(request.GenderFilter, out var genderId))
        {
            query = query.Where(u => u.GenderId == genderId);
        }

        var data = query.Select(u => new { u.CreatedAt }).ToList();
        var result = new List<AnalyticsDataPointDto>();

        if (request.Timeframe.Equals("Day", StringComparison.OrdinalIgnoreCase))
        {
            var currentMonth = DateTime.UtcNow.Month;
            var currentYear = DateTime.UtcNow.Year;
            var daysInMonth = DateTime.DaysInMonth(currentYear, currentMonth);

            for (int i = 1; i <= daysInMonth; i++)
            {
                result.Add(new AnalyticsDataPointDto { Label = i.ToString(), Count = 0 });
            }

            var grouped = data
                .Where(d => d.CreatedAt.Year == currentYear && d.CreatedAt.Month == currentMonth)
                .GroupBy(d => d.CreatedAt.Day)
                .ToDictionary(g => g.Key, g => g.Count());

            foreach (var item in result)
            {
                if (int.TryParse(item.Label, out int day) && grouped.ContainsKey(day))
                {
                    item.Count = grouped[day];
                }
            }
        }
        else if (request.Timeframe.Equals("Month", StringComparison.OrdinalIgnoreCase))
        {
            var now = DateTime.UtcNow;
            for (int i = 11; i >= 0; i--)
            {
                var monthDate = now.AddMonths(-i);
                result.Add(new AnalyticsDataPointDto { Label = monthDate.ToString("MMM yyyy"), Count = 0 });
            }

            var startMonth = now.AddMonths(-11).Date;
            startMonth = new DateTime(startMonth.Year, startMonth.Month, 1);

            var grouped = data
                .Where(d => d.CreatedAt >= startMonth)
                .GroupBy(d => new { d.CreatedAt.Year, d.CreatedAt.Month })
                .ToDictionary(g => new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"), g => g.Count());

            foreach (var item in result)
            {
                if (grouped.ContainsKey(item.Label))
                {
                    item.Count = grouped[item.Label];
                }
            }
        }
        else // Year
        {
            if (!data.Any()) return Result<List<AnalyticsDataPointDto>>.Success(result);

            var minYear = data.Min(d => d.CreatedAt.Year);
            var maxYear = data.Max(d => d.CreatedAt.Year);

            for (int i = minYear; i <= maxYear; i++)
            {
                result.Add(new AnalyticsDataPointDto { Label = i.ToString(), Count = 0 });
            }

            var grouped = data.GroupBy(d => d.CreatedAt.Year).ToDictionary(g => g.Key, g => g.Count());

            foreach (var item in result)
            {
                if (int.TryParse(item.Label, out int year) && grouped.ContainsKey(year))
                {
                    item.Count = grouped[year];
                }
            }
        }

        return Result<List<AnalyticsDataPointDto>>.Success(result);
    }
}

public record GetEngagementAnalyticsQuery(string Timeframe, DateTime DateContext) : IRequest<Result<List<AnalyticsDataPointDto>>>;

public class GetEngagementAnalyticsQueryHandler : IRequestHandler<GetEngagementAnalyticsQuery, Result<List<AnalyticsDataPointDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetEngagementAnalyticsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<List<AnalyticsDataPointDto>>> Handle(GetEngagementAnalyticsQuery request, CancellationToken ct)
    {
        await Task.CompletedTask;
        var result = new List<AnalyticsDataPointDto>();

        if (request.Timeframe.Equals("Day", StringComparison.OrdinalIgnoreCase))
        {
            var date = request.DateContext.Date;
            var nextDate = date.AddDays(1);

            var query = _unitOfWork.Repository<SystemLog>().AsQueryable()
                .Where(l => l.UserId != null && l.Timestamp >= date && l.Timestamp < nextDate);

            var data = query.Select(l => new { l.UserId, l.Timestamp }).ToList();

            for (int i = 0; i < 24; i++)
            {
                result.Add(new AnalyticsDataPointDto { Label = $"{i:00}:00", Count = 0 });
            }

            var grouped = data
                .GroupBy(l => l.Timestamp.Hour)
                .ToDictionary(g => g.Key, g => g.Select(x => x.UserId).Distinct().Count());

            foreach (var item in result)
            {
                var hourStr = item.Label.Split(':')[0];
                if (int.TryParse(hourStr, out int hour) && grouped.ContainsKey(hour))
                {
                    item.Count = grouped[hour];
                }
            }
        }
        else if (request.Timeframe.Equals("Month", StringComparison.OrdinalIgnoreCase))
        {
            var year = request.DateContext.Year;
            var month = request.DateContext.Month;
            var start = new DateTime(year, month, 1);
            var end = start.AddMonths(1);

            var query = _unitOfWork.Repository<SystemLog>().AsQueryable()
                .Where(l => l.UserId != null && l.Timestamp >= start && l.Timestamp < end);

            var data = query.Select(l => new { l.UserId, l.Timestamp }).ToList();

            var daysInMonth = DateTime.DaysInMonth(year, month);
            for (int i = 1; i <= daysInMonth; i++)
            {
                result.Add(new AnalyticsDataPointDto { Label = i.ToString(), Count = 0 });
            }

            var grouped = data
                .GroupBy(l => l.Timestamp.Day)
                .ToDictionary(g => g.Key, g => g.Select(x => x.UserId).Distinct().Count());

            foreach (var item in result)
            {
                if (int.TryParse(item.Label, out int day) && grouped.ContainsKey(day))
                {
                    item.Count = grouped[day];
                }
            }
        }
        else // Year
        {
            var year = request.DateContext.Year;
            var start = new DateTime(year, 1, 1);
            var end = start.AddYears(1);

            var query = _unitOfWork.Repository<SystemLog>().AsQueryable()
                .Where(l => l.UserId != null && l.Timestamp >= start && l.Timestamp < end);

            var data = query.Select(l => new { l.UserId, l.Timestamp }).ToList();

            for (int i = 1; i <= 12; i++)
            {
                var mDate = new DateTime(year, i, 1);
                result.Add(new AnalyticsDataPointDto { Label = mDate.ToString("MMM"), Count = 0 });
            }

            var grouped = data
                .GroupBy(l => l.Timestamp.Month)
                .ToDictionary(g => g.Key, g => g.Select(x => x.UserId).Distinct().Count());

            foreach (var item in result)
            {
                var mDate = DateTime.ParseExact(item.Label, "MMM", System.Globalization.CultureInfo.InvariantCulture);
                if (grouped.ContainsKey(mDate.Month))
                {
                    item.Count = grouped[mDate.Month];
                }
            }
        }

        return Result<List<AnalyticsDataPointDto>>.Success(result);
    }
}
