using WhereIsTheTrain.Domain.Enums;

namespace WhereIsTheTrain.Application.Features.Trips.DTOs;

public class TripStatusLookupDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
}

public class TripDto
{
    public Guid Id { get; set; }
    public Guid TrainId { get; set; }
    public string TrainNumber { get; set; } = string.Empty;
    public string TrainNameAr { get; set; } = string.Empty;
    public string TrainNameEn { get; set; } = string.Empty;
    public DateOnly TripDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public TripStatusLookupDto StatusDetails { get; set; } = null!;
    public DateTime? ActualDeparture { get; set; }
    public DateTime? ActualArrival { get; set; }
    public int FollowerCount { get; set; }
    public bool IsFollowedByCurrentUser { get; set; }
    public bool IsNotificationsEnabled { get; set; }
    public List<double[]>? RoutePath { get; set; }
    public Guid? TrainTypeId { get; set; }
    public string? TrainTypeNameAr { get; set; }
    public string? TrainTypeNameEn { get; set; }
    public string? MarkerPngUrl { get; set; }
    /// <summary>Scheduled departure of the first route stop — used for sorting today's trips by start time.</summary>
    public TimeSpan? ScheduledDeparture { get; set; }
}

public class TripDetailDto : TripDto
{
    public List<RouteStopDto> RouteStops { get; set; } = new();
    public List<LiveUpdateDto> RecentUpdates { get; set; } = new();
    public List<double[]>? RoutePath { get; set; }
}

public class RouteStopDto
{
    public Guid StopId { get; set; }
    public string StopNameAr { get; set; } = string.Empty;
    public string StopNameEn { get; set; } = string.Empty;
    public string StopCode { get; set; } = string.Empty;
    public string? CityAr { get; set; }
    public string? CityEn { get; set; }
    public int StopOrder { get; set; }
    public TimeSpan? ScheduledArrival { get; set; }
    public TimeSpan? ScheduledDeparture { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class LiveUpdateDto
{
    public Guid Id { get; set; }
    public Guid TripId { get; set; }
    public Guid? AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string? AuthorAvatarUrl { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? StatusTag { get; set; }
    public string? CrowdState { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ThanksCount { get; set; }
    public bool IsThankedByCurrentUser { get; set; }
    public bool IsRemovalRequested { get; set; }
}

public class FollowedTripDto : TripDto
{
    public string PersonalStatus { get; set; } = string.Empty;
    public DateTime FollowedAt { get; set; }
}

public class TrainDto
{
    public Guid Id { get; set; }
    public string TrainNumber { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }
    public bool IsActive { get; set; }
    public int FollowerCount { get; set; }
    public string? PathCode { get; set; }
    public string? PathNameAr { get; set; }
    public string? PathNameEn { get; set; }
    public List<RouteStopDto> RouteStops { get; set; } = new();
    public List<double[]>? RoutePath { get; set; }
    public Guid? TrainTypeId { get; set; }
    public string? TrainTypeNameAr { get; set; }
    public string? TrainTypeNameEn { get; set; }
    public string? MarkerPngUrl { get; set; }
}

public class DashboardDto
{
    public int TotalUsers { get; set; }
    public int TotalTrains { get; set; }
    public int ActiveTripsToday { get; set; }
    public int PendingTripsToday { get; set; }
    public int RunningTripsToday { get; set; }
    public int TotalLiveUpdatesToday { get; set; }
    public List<LiveUpdateDto> RecentUpdates { get; set; } = new();
}

public class CreateLiveUpdateDto
{
    public string Content { get; set; } = string.Empty;
    public string? StatusTag { get; set; }
    public string? CrowdState { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
