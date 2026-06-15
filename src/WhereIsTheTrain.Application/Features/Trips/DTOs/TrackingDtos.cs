using System;
using System.Collections.Generic;

namespace WhereIsTheTrain.Application.Features.Trips.DTOs;

public class TelemetryResponseDto
{
    public Guid TripId { get; set; }
    public double SnappedLatitude { get; set; }
    public double SnappedLongitude { get; set; }
    public double? RawLatitude { get; set; }
    public double? RawLongitude { get; set; }
    public double Speed { get; set; } // Speed in km/h
    public double DistanceAlongRoute { get; set; } // Snapped cumulative distance along route in meters
    public List<UpcomingStopTrackingDto> UpcomingStops { get; set; } = new();
}

public class UpcomingStopTrackingDto
{
    public Guid StopId { get; set; }
    public string StopNameAr { get; set; } = string.Empty;
    public string StopNameEn { get; set; } = string.Empty;
    public string StopCode { get; set; } = string.Empty;
    public double DistanceRemaining { get; set; } // Remaining distance in meters
    public DateTime EstimatedTimeOfArrival { get; set; }
}
