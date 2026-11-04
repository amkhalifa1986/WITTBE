using System.Globalization;
using System.Linq.Expressions;
using MediatR;
using WhereIsTheTrain.Application.Common;
using WhereIsTheTrain.Application.Features.Trips.DTOs;
using WhereIsTheTrain.Domain.Entities;
using WhereIsTheTrain.Domain.Interfaces;

namespace WhereIsTheTrain.Application.Features.Admin;

/// <summary>
/// One page of trips for the admin Trips screen. Filtering, sorting and paging run in the
/// database, so the screen stays fast however many trips accumulate over time.
/// </summary>
/// <param name="Search">Matches train number or name, or a date typed as yyyy, yyyy-MM or yyyy-MM-dd.</param>
public record GetTripsPageQuery(int Page, int PageSize, string? Search, Guid? TrainTypeId)
    : IRequest<Result<PagedResult<TripDto>>>;

public class GetTripsPageQueryHandler : IRequestHandler<GetTripsPageQuery, Result<PagedResult<TripDto>>>
{
    private const int MaxPageSize = 100;
    private readonly IUnitOfWork _unitOfWork;

    public GetTripsPageQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<PagedResult<TripDto>>> Handle(GetTripsPageQuery request, CancellationToken ct)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, MaxPageSize);
        var filter = BuildFilter(request.Search, request.TrainTypeId);
        var trips = _unitOfWork.Repository<Trip>();

        var totalCount = await trips.CountAsync(filter, ct);

        var rows = await trips.QueryAsync(q => q
            .Where(filter)
            .OrderByDescending(t => t.TripDate)
            .ThenBy(t => t.Train.TrainNumber)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new
            {
                t.Id,
                t.TrainId,
                t.TripDate,
                t.StatusId,
                t.ActualDeparture,
                t.ActualArrival,
                t.Train.TrainNumber,
                t.Train.NameAr,
                t.Train.NameEn,
                t.Train.TrainTypeId
            }), ct);

        var tripIds = rows.Select(r => r.Id).ToList();
        var followerCounts = tripIds.Count == 0
            ? new Dictionary<Guid, int>()
            : (await _unitOfWork.Repository<TripFollower>().QueryAsync(q => q
                    .Where(f => tripIds.Contains(f.TripId))
                    .GroupBy(f => f.TripId)
                    .Select(g => new { TripId = g.Key, Count = g.Count() }), ct))
                .ToDictionary(x => x.TripId, x => x.Count);
        var trainTypes = (await _unitOfWork.Repository<TrainType>().GetAllAsync(ct)).ToDictionary(t => t.Id);
        var statuses = (await _unitOfWork.Repository<TripStatusLookup>().GetAllAsync(ct)).ToDictionary(s => s.Id);

        var items = rows.Select(r =>
        {
            var trainType = r.TrainTypeId.HasValue && trainTypes.TryGetValue(r.TrainTypeId.Value, out var tt) ? tt : null;
            var status = statuses.TryGetValue(r.StatusId, out var sl) ? sl : null;
            return new TripDto
            {
                Id = r.Id,
                TrainId = r.TrainId,
                TrainNumber = r.TrainNumber ?? string.Empty,
                TrainNameAr = r.NameAr ?? string.Empty,
                TrainNameEn = r.NameEn ?? string.Empty,
                TripDate = r.TripDate,
                Status = status?.Code ?? "Scheduled",
                StatusDetails = status != null ? new TripStatusLookupDto
                {
                    Id = status.Id,
                    Code = status.Code,
                    NameEn = status.NameEn,
                    NameAr = status.NameAr,
                    Color = status.Color
                } : new TripStatusLookupDto(),
                ActualDeparture = r.ActualDeparture,
                ActualArrival = r.ActualArrival,
                FollowerCount = followerCounts.TryGetValue(r.Id, out var count) ? count : 0,
                TrainTypeId = r.TrainTypeId,
                TrainTypeNameAr = trainType?.NameAr,
                TrainTypeNameEn = trainType?.NameEn,
                MarkerPngUrl = trainType?.MarkerPngUrl
            };
        }).ToList();

        return Result<PagedResult<TripDto>>.Success(new PagedResult<TripDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    private static Expression<Func<Trip, bool>> BuildFilter(string? search, Guid? trainTypeId)
    {
        var term = search?.Trim() ?? string.Empty;
        var hasTerm = term.Length > 0;
        var (hasDate, from, to) = ParseDateRange(term);
        var hasType = trainTypeId.HasValue;
        var typeId = trainTypeId ?? Guid.Empty;

        return t =>
            (!hasType || t.Train.TrainTypeId == typeId) &&
            (!hasTerm ||
                t.Train.TrainNumber.Contains(term) ||
                t.Train.NameEn.Contains(term) ||
                t.Train.NameAr.Contains(term) ||
                (hasDate && t.TripDate >= from && t.TripDate < to));
    }

    /// <summary>A typed year, year-month or full date becomes the matching date range.</summary>
    private static (bool hasDate, DateOnly from, DateOnly to) ParseDateRange(string term)
    {
        var culture = CultureInfo.InvariantCulture;
        if (DateOnly.TryParseExact(term, "yyyy-MM-dd", culture, DateTimeStyles.None, out var day))
            return (true, day, day.AddDays(1));
        if (DateOnly.TryParseExact(term + "-01", "yyyy-MM-dd", culture, DateTimeStyles.None, out var month))
            return (true, month, month.AddMonths(1));
        if (term.Length == 4 && int.TryParse(term, NumberStyles.None, culture, out var year) && year is >= 1 and <= 9998)
            return (true, new DateOnly(year, 1, 1), new DateOnly(year + 1, 1, 1));
        return (false, default, default);
    }
}
