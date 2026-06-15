using WhereIsTheTrain.Domain.Common;

namespace WhereIsTheTrain.Domain.Entities;

public class TripTelemetry : AuditableEntity
{
    public Guid TripId { get; set; }
    public Guid UserId { get; set; }
    public double RawLatitude { get; set; }
    public double RawLongitude { get; set; }
    public double SnappedLatitude { get; set; }
    public double SnappedLongitude { get; set; }
    public double Speed { get; set; } // Speed in m/s
    public double DistanceAlongRoute { get; set; } // Distance in meters from polyline start
    public DateTime Timestamp { get; set; }

    // Navigation properties
    public Trip Trip { get; set; } = null!;
    public User User { get; set; } = null!;
}
