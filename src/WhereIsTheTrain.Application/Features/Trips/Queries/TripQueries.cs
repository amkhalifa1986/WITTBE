using MediatR;
using Microsoft.Extensions.Caching.Memory;
using WhereIsTheTrain.Application.Common;
using WhereIsTheTrain.Application.Features.Trips.DTOs;
using WhereIsTheTrain.Domain.Interfaces;
using WhereIsTheTrain.Domain.Entities;
using WhereIsTheTrain.Domain.Enums;

namespace WhereIsTheTrain.Application.Features.Trips.Queries;

// --- Get Today's Trips ---
public record GetTodayTripsQuery(Guid? CurrentUserId = null) : IRequest<Result<List<TripDto>>>;

public class GetTodayTripsQueryHandler : IRequestHandler<GetTodayTripsQuery, Result<List<TripDto>>>
{
    private readonly ITripRepository _tripRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;

    public GetTodayTripsQueryHandler(ITripRepository tripRepo, IUnitOfWork unitOfWork, IMemoryCache cache)
    {
        _tripRepo = tripRepo;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<Result<List<TripDto>>> Handle(GetTodayTripsQuery request, CancellationToken ct)
    {
        var trips = await _tripRepo.GetTodayTripsAsync(ct);

        // Cache railway paths — they are static reference data, expensive to load
        var railwayPaths = await _cache.GetOrCreateAsync("railway_paths", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            return await _unitOfWork.RailwayPaths.GetAllWithStopsAsync(ct);
        }) ?? new List<WhereIsTheTrain.Domain.Entities.RailwayPath>();

        var dtos = trips.Select(t => {
            // Cache computed geometry per train — CPU-intensive, deterministic per route
            var routePath = _cache.GetOrCreate($"route_path_{t.TrainId}", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                return RoutePathBuilder.BuildRoutePath(t.Train.RouteStops, railwayPaths);
            })!;
            return new TripDto
            {
                Id = t.Id,
                TrainId = t.TrainId,
                TrainNumber = t.Train.TrainNumber,
                TrainNameAr = t.Train.NameAr,
                TrainNameEn = t.Train.NameEn,
                TripDate = t.TripDate,
                Status = t.Status?.Code ?? "Scheduled",
                StatusDetails = t.Status != null ? new TripStatusLookupDto
                {
                    Id = t.Status.Id,
                    Code = t.Status.Code,
                    NameEn = t.Status.NameEn,
                    NameAr = t.Status.NameAr,
                    Color = t.Status.Color
                } : new TripStatusLookupDto(),
                ActualDeparture = t.ActualDeparture,
                ActualArrival = t.ActualArrival,
                FollowerCount = t.Followers?.Count ?? 0,
                IsFollowedByCurrentUser = request.CurrentUserId.HasValue && (t.Followers?.Any(f => f.UserId == request.CurrentUserId.Value) ?? false),
                IsNotificationsEnabled = request.CurrentUserId.HasValue && (t.Followers?.Any(f => f.UserId == request.CurrentUserId.Value && f.NotificationsEnabled) ?? false),
                RoutePath = routePath.Coordinates.Select(c => new double[] { c.Y, c.X }).ToList(),
                TrainTypeId = t.Train.TrainTypeId,
                TrainTypeNameAr = t.Train.TrainType?.NameAr,
                TrainTypeNameEn = t.Train.TrainType?.NameEn,
                MarkerPngUrl = t.Train.TrainType?.MarkerPngUrl,
                // First stop's scheduled departure = trip start time
                ScheduledDeparture = t.Train.RouteStops?
                    .OrderBy(rs => rs.ScheduledDeparture)
                    .Select(rs => rs.ScheduledDeparture)
                    .FirstOrDefault()
            };
        }).ToList();

        return Result<List<TripDto>>.Success(dtos);
    }
}

// --- Get Trip Details ---
public record GetTripDetailsQuery(Guid TripId, Guid? CurrentUserId = null) : IRequest<Result<TripDetailDto>>;

public class GetTripDetailsQueryHandler : IRequestHandler<GetTripDetailsQuery, Result<TripDetailDto>>
{
    private readonly ITripRepository _tripRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;

    public GetTripDetailsQueryHandler(ITripRepository tripRepo, IUnitOfWork unitOfWork, IMemoryCache cache)
    {
        _tripRepo = tripRepo;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<Result<TripDetailDto>> Handle(GetTripDetailsQuery request, CancellationToken ct)
    {
        var trip = await _tripRepo.GetWithDetailsAsync(request.TripId, ct);
        if (trip == null)
            return Result<TripDetailDto>.Failure("Trip not found.", 404);

        // Cache railway paths — they are static reference data, expensive to load
        var railwayPaths = await _cache.GetOrCreateAsync("railway_paths", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            return await _unitOfWork.RailwayPaths.GetAllWithStopsAsync(ct);
        }) ?? new List<WhereIsTheTrain.Domain.Entities.RailwayPath>();

        // Cache computed geometry per train — CPU-intensive, deterministic per route
        var routePath = _cache.GetOrCreate($"route_path_{trip.TrainId}", entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
            return RoutePathBuilder.BuildRoutePath(trip.Train.RouteStops, railwayPaths);
        })!;

        var dto = new TripDetailDto
        {
            Id = trip.Id,
            TrainId = trip.TrainId,
            TrainNumber = trip.Train.TrainNumber,
            TrainNameAr = trip.Train.NameAr,
            TrainNameEn = trip.Train.NameEn,
            TripDate = trip.TripDate,
            Status = trip.Status?.Code ?? "Scheduled",
            StatusDetails = trip.Status != null ? new TripStatusLookupDto
            {
                Id = trip.Status.Id,
                Code = trip.Status.Code,
                NameEn = trip.Status.NameEn,
                NameAr = trip.Status.NameAr,
                Color = trip.Status.Color
            } : new TripStatusLookupDto(),
            ActualDeparture = trip.ActualDeparture,
            ActualArrival = trip.ActualArrival,
            FollowerCount = trip.Followers?.Count ?? 0,
            IsFollowedByCurrentUser = request.CurrentUserId.HasValue && (trip.Followers?.Any(f => f.UserId == request.CurrentUserId.Value) ?? false),
            IsNotificationsEnabled = request.CurrentUserId.HasValue && (trip.Followers?.Any(f => f.UserId == request.CurrentUserId.Value && f.NotificationsEnabled) ?? false),
            TrainTypeId = trip.Train.TrainTypeId,
            TrainTypeNameAr = trip.Train.TrainType?.NameAr,
            TrainTypeNameEn = trip.Train.TrainType?.NameEn,
            MarkerPngUrl = trip.Train.TrainType?.MarkerPngUrl,
            RouteStops = trip.Train.RouteStops?.OrderBy(rs => rs.StopOrder).Select(rs => new RouteStopDto
            {
                StopId = rs.Stop.Id,
                StopNameAr = rs.Stop.NameAr,
                StopNameEn = rs.Stop.NameEn,
                StopCode = rs.Stop.Code,
                CityAr = rs.Stop.City?.NameAr,
                CityEn = rs.Stop.City?.NameEn,
                StopOrder = rs.StopOrder,
                ScheduledArrival = rs.ScheduledArrival,
                ScheduledDeparture = rs.ScheduledDeparture,
                Latitude = rs.Stop.Latitude,
                Longitude = rs.Stop.Longitude
            }).ToList() ?? new(),
            RecentUpdates = trip.LiveUpdates?.Where(u => u.IsApproved).Select(u => new LiveUpdateDto
            {
                Id = u.Id,
                TripId = u.TripId,
                AuthorId = u.AuthorId,
                AuthorName = u.Author != null ? u.Author.DisplayName : "System",
                AuthorAvatarUrl = u.Author?.AvatarUrl,
                Content = u.Content,
                StatusTag = u.StatusTag?.ToString(),
                CrowdState = u.CrowdState?.ToString(),
                Latitude = u.Latitude,
                Longitude = u.Longitude,
                CreatedAt = u.CreatedAt,
                ThanksCount = u.ThanksList?.Count ?? 0,
                IsThankedByCurrentUser = request.CurrentUserId.HasValue && u.ThanksList != null && u.ThanksList.Any(t => t.UserId == request.CurrentUserId.Value),
                IsRemovalRequested = u.IsRemovalRequested
            }).ToList() ?? new(),
            RoutePath = routePath.Coordinates.Select(c => new double[] { c.Y, c.X }).ToList()
        };

        return Result<TripDetailDto>.Success(dto);
    }
}

// --- Get Followed Trips ---
public record GetFollowedTripsQuery(Guid UserId) : IRequest<Result<List<FollowedTripDto>>>;

public class GetFollowedTripsQueryHandler : IRequestHandler<GetFollowedTripsQuery, Result<List<FollowedTripDto>>>
{
    private readonly ITripRepository _tripRepo;

    public GetFollowedTripsQueryHandler(ITripRepository tripRepo) => _tripRepo = tripRepo;

    public async Task<Result<List<FollowedTripDto>>> Handle(GetFollowedTripsQuery request, CancellationToken ct)
    {
        var trips = await _tripRepo.GetFollowedTripsAsync(request.UserId, ct);
        var dtos = trips.Select(t =>
        {
            var follower = t.Followers?.FirstOrDefault(f => f.UserId == request.UserId);
            return new FollowedTripDto
            {
                Id = t.Id,
                TrainId = t.TrainId,
                TrainNumber = t.Train.TrainNumber,
                TrainNameAr = t.Train.NameAr,
                TrainNameEn = t.Train.NameEn,
                TripDate = t.TripDate,
                Status = t.Status?.Code ?? "Scheduled",
                StatusDetails = t.Status != null ? new TripStatusLookupDto
                {
                    Id = t.Status.Id,
                    Code = t.Status.Code,
                    NameEn = t.Status.NameEn,
                    NameAr = t.Status.NameAr,
                    Color = t.Status.Color
                } : new TripStatusLookupDto(),
                ActualDeparture = t.ActualDeparture,
                ActualArrival = t.ActualArrival,
                FollowerCount = t.Followers?.Count ?? 0,
                IsFollowedByCurrentUser = true,
                IsNotificationsEnabled = follower?.NotificationsEnabled ?? false,
                PersonalStatus = follower?.PersonalStatus.ToString() ?? "Following",
                FollowedAt = follower?.FollowedAt ?? DateTime.UtcNow
            };
        }).ToList();

        return Result<List<FollowedTripDto>>.Success(dtos);
    }
}

// --- Search Train By Number ---
public record SearchTrainByNumberQuery(string SearchTerm) : IRequest<Result<List<TrainDto>>>;

public class SearchTrainByNumberQueryHandler : IRequestHandler<SearchTrainByNumberQuery, Result<List<TrainDto>>>
{
    private readonly ITrainRepository _trainRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;

    public SearchTrainByNumberQueryHandler(ITrainRepository trainRepo, IUnitOfWork unitOfWork, IMemoryCache cache)
    {
        _trainRepo = trainRepo;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<Result<List<TrainDto>>> Handle(SearchTrainByNumberQuery request, CancellationToken ct)
    {
        var trains = await _trainRepo.SearchByNumberAsync(request.SearchTerm, ct);

        // Cache railway paths — they are static reference data, expensive to load
        var railwayPaths = await _cache.GetOrCreateAsync("railway_paths", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            return await _unitOfWork.RailwayPaths.GetAllWithStopsAsync(ct);
        }) ?? new List<WhereIsTheTrain.Domain.Entities.RailwayPath>();

        var trainTypes = await _unitOfWork.Repository<TrainType>().GetAllAsync(ct);
        var trainTypesDict = trainTypes.ToDictionary(tt => tt.Id);

        var dtos = trains.Select(t => {
            // Cache computed geometry per train — CPU-intensive, deterministic per route
            var routePath = _cache.GetOrCreate($"route_path_{t.Id}", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                return RoutePathBuilder.BuildRoutePath(t.RouteStops, railwayPaths);
            })!;
            var coveringPaths = RoutePathBuilder.GetCoveringPaths(t.RouteStops, railwayPaths);
            var typeInfo = t.TrainTypeId.HasValue && trainTypesDict.TryGetValue(t.TrainTypeId.Value, out var ttVal) ? ttVal : null;

            return new TrainDto
            {
                Id = t.Id,
                TrainNumber = t.TrainNumber,
                NameAr = t.NameAr,
                NameEn = t.NameEn,
                DescriptionAr = t.DescriptionAr,
                DescriptionEn = t.DescriptionEn,
                IsActive = t.IsActive,
                PathCode = string.Join(", ", coveringPaths.Select(p => p.Code)),
                PathNameAr = string.Join(" + ", coveringPaths.Select(p => p.NameAr)),
                PathNameEn = string.Join(" + ", coveringPaths.Select(p => p.NameEn)),
                RouteStops = t.RouteStops?.OrderBy(rs => rs.StopOrder).Select(rs => new RouteStopDto
                {
                    StopId = rs.Stop.Id,
                    StopNameAr = rs.Stop.NameAr,
                    StopNameEn = rs.Stop.NameEn,
                    StopCode = rs.Stop.Code,
                    CityAr = rs.Stop.City?.NameAr,
                    CityEn = rs.Stop.City?.NameEn,
                    StopOrder = rs.StopOrder,
                    ScheduledArrival = rs.ScheduledArrival,
                    ScheduledDeparture = rs.ScheduledDeparture,
                    Latitude = rs.Stop.Latitude,
                    Longitude = rs.Stop.Longitude
                }).ToList() ?? new(),
                RoutePath = routePath.Coordinates.Select(c => new double[] { c.Y, c.X }).ToList(),
                TrainTypeId = t.TrainTypeId,
                TrainTypeNameAr = typeInfo?.NameAr,
                TrainTypeNameEn = typeInfo?.NameEn,
                MarkerPngUrl = typeInfo?.MarkerPngUrl
            };
        }).ToList();

        return Result<List<TrainDto>>.Success(dtos);
    }
}

// --- Search Train By Stops ---
public record SearchTrainByStopsQuery(string FromStop, string ToStop) : IRequest<Result<List<TrainDto>>>;

public class SearchTrainByStopsQueryHandler : IRequestHandler<SearchTrainByStopsQuery, Result<List<TrainDto>>>
{
    private readonly ITrainRepository _trainRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;

    public SearchTrainByStopsQueryHandler(ITrainRepository trainRepo, IUnitOfWork unitOfWork, IMemoryCache cache)
    {
        _trainRepo = trainRepo;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<Result<List<TrainDto>>> Handle(SearchTrainByStopsQuery request, CancellationToken ct)
    {
        var trains = await _trainRepo.SearchByStopsAsync(request.FromStop, request.ToStop, ct);

        // Cache railway paths — they are static reference data, expensive to load
        var railwayPaths = await _cache.GetOrCreateAsync("railway_paths", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            return await _unitOfWork.RailwayPaths.GetAllWithStopsAsync(ct);
        }) ?? new List<WhereIsTheTrain.Domain.Entities.RailwayPath>();

        var trainTypes = await _unitOfWork.Repository<TrainType>().GetAllAsync(ct);
        var trainTypesDict = trainTypes.ToDictionary(tt => tt.Id);

        var dtos = trains.Select(t => {
            // Cache computed geometry per train — CPU-intensive, deterministic per route
            var routePath = _cache.GetOrCreate($"route_path_{t.Id}", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                return RoutePathBuilder.BuildRoutePath(t.RouteStops, railwayPaths);
            })!;
            var coveringPaths = RoutePathBuilder.GetCoveringPaths(t.RouteStops, railwayPaths);
            var typeInfo = t.TrainTypeId.HasValue && trainTypesDict.TryGetValue(t.TrainTypeId.Value, out var ttVal) ? ttVal : null;

            return new TrainDto
            {
                Id = t.Id,
                TrainNumber = t.TrainNumber,
                NameAr = t.NameAr,
                NameEn = t.NameEn,
                DescriptionAr = t.DescriptionAr,
                DescriptionEn = t.DescriptionEn,
                IsActive = t.IsActive,
                PathCode = string.Join(", ", coveringPaths.Select(p => p.Code)),
                PathNameAr = string.Join(" + ", coveringPaths.Select(p => p.NameAr)),
                PathNameEn = string.Join(" + ", coveringPaths.Select(p => p.NameEn)),
                RouteStops = t.RouteStops?.OrderBy(rs => rs.StopOrder).Select(rs => new RouteStopDto
                {
                    StopId = rs.Stop.Id,
                    StopNameAr = rs.Stop.NameAr,
                    StopNameEn = rs.Stop.NameEn,
                    StopCode = rs.Stop.Code,
                    CityAr = rs.Stop.City?.NameAr,
                    CityEn = rs.Stop.City?.NameEn,
                    StopOrder = rs.StopOrder,
                    ScheduledArrival = rs.ScheduledArrival,
                    ScheduledDeparture = rs.ScheduledDeparture,
                    Latitude = rs.Stop.Latitude,
                    Longitude = rs.Stop.Longitude
                }).ToList() ?? new(),
                RoutePath = routePath.Coordinates.Select(c => new double[] { c.Y, c.X }).ToList(),
                TrainTypeId = t.TrainTypeId,
                TrainTypeNameAr = typeInfo?.NameAr,
                TrainTypeNameEn = typeInfo?.NameEn,
                MarkerPngUrl = typeInfo?.MarkerPngUrl
            };
        }).ToList();

        return Result<List<TrainDto>>.Success(dtos);
    }
}

// --- Get Train Details ---
public record GetTrainDetailsQuery(Guid TrainId) : IRequest<Result<TrainDto>>;

public class GetTrainDetailsQueryHandler : IRequestHandler<GetTrainDetailsQuery, Result<TrainDto>>
{
    private readonly ITrainRepository _trainRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;

    public GetTrainDetailsQueryHandler(ITrainRepository trainRepo, IUnitOfWork unitOfWork, IMemoryCache cache)
    {
        _trainRepo = trainRepo;
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<Result<TrainDto>> Handle(GetTrainDetailsQuery request, CancellationToken ct)
    {
        var train = await _trainRepo.GetWithRouteAsync(request.TrainId, ct);
        if (train == null)
            return Result<TrainDto>.Failure("Train not found.", 404);

        // Cache railway paths — they are static reference data, expensive to load
        var railwayPaths = await _cache.GetOrCreateAsync("railway_paths", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            return await _unitOfWork.RailwayPaths.GetAllWithStopsAsync(ct);
        }) ?? new List<WhereIsTheTrain.Domain.Entities.RailwayPath>();

        // Cache computed geometry per train — CPU-intensive, deterministic per route
        var routePath = _cache.GetOrCreate($"route_path_{train.Id}", entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
            return RoutePathBuilder.BuildRoutePath(train.RouteStops, railwayPaths);
        })!;
        var coveringPaths = RoutePathBuilder.GetCoveringPaths(train.RouteStops, railwayPaths);

        var trainType = train.TrainTypeId.HasValue
            ? await _unitOfWork.Repository<TrainType>().GetByIdAsync(train.TrainTypeId.Value, ct)
            : null;

        var dto = new TrainDto
        {
            Id = train.Id,
            TrainNumber = train.TrainNumber,
            NameAr = train.NameAr,
            NameEn = train.NameEn,
            DescriptionAr = train.DescriptionAr,
            DescriptionEn = train.DescriptionEn,
            IsActive = train.IsActive,
            PathCode = string.Join(", ", coveringPaths.Select(p => p.Code)),
            PathNameAr = string.Join(" + ", coveringPaths.Select(p => p.NameAr)),
            PathNameEn = string.Join(" + ", coveringPaths.Select(p => p.NameEn)),
            RouteStops = train.RouteStops?.OrderBy(rs => rs.StopOrder).Select(rs => new RouteStopDto
            {
                StopId = rs.Stop.Id,
                StopNameAr = rs.Stop.NameAr,
                StopNameEn = rs.Stop.NameEn,
                StopCode = rs.Stop.Code,
                CityAr = rs.Stop.City?.NameAr,
                CityEn = rs.Stop.City?.NameEn,
                StopOrder = rs.StopOrder,
                ScheduledArrival = rs.ScheduledArrival,
                ScheduledDeparture = rs.ScheduledDeparture,
                Latitude = rs.Stop.Latitude,
                Longitude = rs.Stop.Longitude
            }).ToList() ?? new(),
            RoutePath = routePath.Coordinates.Select(c => new double[] { c.Y, c.X }).ToList(),
            TrainTypeId = train.TrainTypeId,
            TrainTypeNameAr = trainType?.NameAr,
            TrainTypeNameEn = trainType?.NameEn,
            MarkerPngUrl = trainType?.MarkerPngUrl
        };

        return Result<TrainDto>.Success(dto);
    }
}

// --- Get Dashboard Stats ---
public record GetDashboardStatsQuery() : IRequest<Result<DashboardDto>>;

public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, Result<DashboardDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetDashboardStatsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<DashboardDto>> Handle(GetDashboardStatsQuery request, CancellationToken ct)
    {
        var today = WhereIsTheTrain.Domain.Common.DateHelper.GetEgyptToday();
        var totalUsers = await _unitOfWork.Users.CountAsync(cancellationToken: ct);
        var totalTrains = await _unitOfWork.Trains.CountAsync(t => t.IsActive, ct);
        var activeTripsToday = await _unitOfWork.Trips.CountAsync(t => t.TripDate == today, ct);
        var pendingTripsToday = await _unitOfWork.Trips.CountAsync(t => t.TripDate == today && t.StatusId == TripStatuses.Scheduled, ct);
        var runningTripsToday = await _unitOfWork.Trips.CountAsync(t => t.TripDate == today && (t.StatusId == TripStatuses.Departed || t.StatusId == TripStatuses.InTransit || t.StatusId == TripStatuses.Delayed), ct);
        var todayStart = DateTime.UtcNow.Date;
        var totalUpdatesToday = await _unitOfWork.Repository<Domain.Entities.TripLiveUpdate>()
            .CountAsync(u => u.CreatedAt >= todayStart && u.IsApproved, ct);

        // Load approved updates then take top 5 by recency
        var updates = await _unitOfWork.Repository<TripLiveUpdate>().FindAsync(u => u.IsApproved, ct);
        var top5 = updates.OrderByDescending(x => x.CreatedAt).Take(5).ToList();

        var authorDict = new Dictionary<Guid, Domain.Entities.User>();
        foreach (var u in top5.Where(u => u.AuthorId.HasValue))
        {
            if (!authorDict.ContainsKey(u.AuthorId!.Value))
            {
                var user = await _unitOfWork.Users.GetByIdAsync(u.AuthorId.Value, ct);
                if (user != null)
                {
                    authorDict[user.Id] = user;
                }
            }
        }

        var recentUpdates = top5.Select(u =>
        {
            var author = u.AuthorId.HasValue && authorDict.TryGetValue(u.AuthorId.Value, out var a) ? a : null;
            return new LiveUpdateDto
            {
                Id = u.Id,
                TripId = u.TripId,
                AuthorId = u.AuthorId,
                AuthorName = author?.DisplayName ?? "System",
                AuthorAvatarUrl = author?.AvatarUrl,
                Content = u.Content,
                StatusTag = u.StatusTag?.ToString(),
                CrowdState = u.CrowdState?.ToString(),
                Latitude = u.Latitude,
                Longitude = u.Longitude,
                CreatedAt = u.CreatedAt,
                ThanksCount = u.ThanksList?.Count ?? 0,
                IsThankedByCurrentUser = false,
                IsRemovalRequested = u.IsRemovalRequested
            };
        }).ToList();

        return Result<DashboardDto>.Success(new DashboardDto
        {
            TotalUsers = totalUsers,
            TotalTrains = totalTrains,
            ActiveTripsToday = activeTripsToday,
            PendingTripsToday = pendingTripsToday,
            RunningTripsToday = runningTripsToday,
            TotalLiveUpdatesToday = totalUpdatesToday,
            RecentUpdates = recentUpdates
        });
    }
}

// --- Get Train's Trips ---
public record GetTrainTripsQuery(Guid TrainId, Guid? CurrentUserId = null) : IRequest<Result<List<TripDto>>>;

public class GetTrainTripsQueryHandler : IRequestHandler<GetTrainTripsQuery, Result<List<TripDto>>>
{
    private readonly ITripRepository _tripRepo;

    public GetTrainTripsQueryHandler(ITripRepository tripRepo) => _tripRepo = tripRepo;

    public async Task<Result<List<TripDto>>> Handle(GetTrainTripsQuery request, CancellationToken ct)
    {
        var trips = await _tripRepo.GetTripsByTrainIdAsync(request.TrainId, ct);
        var dtos = trips.Select(t => new TripDto
        {
            Id = t.Id,
            TrainId = t.TrainId,
            TrainNumber = t.Train.TrainNumber,
            TrainNameAr = t.Train.NameAr,
            TrainNameEn = t.Train.NameEn,
            TripDate = t.TripDate,
            Status = t.Status?.Code ?? "Scheduled",
            StatusDetails = t.Status != null ? new TripStatusLookupDto
            {
                Id = t.Status.Id,
                Code = t.Status.Code,
                NameEn = t.Status.NameEn,
                NameAr = t.Status.NameAr,
                Color = t.Status.Color
            } : new TripStatusLookupDto(),
            ActualDeparture = t.ActualDeparture,
            ActualArrival = t.ActualArrival,
            FollowerCount = t.Followers?.Count ?? 0,
            IsFollowedByCurrentUser = request.CurrentUserId.HasValue && (t.Followers?.Any(f => f.UserId == request.CurrentUserId.Value) ?? false),
            IsNotificationsEnabled = request.CurrentUserId.HasValue && (t.Followers?.Any(f => f.UserId == request.CurrentUserId.Value && f.NotificationsEnabled) ?? false),
            TrainTypeId = t.Train.TrainTypeId,
            TrainTypeNameAr = t.Train.TrainType?.NameAr,
            TrainTypeNameEn = t.Train.TrainType?.NameEn,
            MarkerPngUrl = t.Train.TrainType?.MarkerPngUrl
        }).ToList();

        return Result<List<TripDto>>.Success(dtos);
    }
}

