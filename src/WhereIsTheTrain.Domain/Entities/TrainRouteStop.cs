using WhereIsTheTrain.Domain.Common;

namespace WhereIsTheTrain.Domain.Entities;

public class TrainRouteStop : BaseEntity
{
    public Guid TrainId { get; set; }
    public Guid StopId { get; set; }
    public int StopOrder { get; set; }
    public TimeSpan? ScheduledArrival { get; set; }
    public TimeSpan? ScheduledDeparture { get; set; }
    public double DistanceAlongRoute { get; set; }

    // Navigation properties
    public Train Train { get; set; } = null!;
    public Stop Stop { get; set; } = null!;
}
