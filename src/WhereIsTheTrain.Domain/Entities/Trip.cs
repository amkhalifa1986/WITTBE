using WhereIsTheTrain.Domain.Common;
using WhereIsTheTrain.Domain.Enums;

namespace WhereIsTheTrain.Domain.Entities;

public class Trip : AuditableEntity
{
    public Guid TrainId { get; set; }
    public DateOnly TripDate { get; set; }
    public Guid StatusId { get; set; }
    public TripStatusLookup Status { get; set; } = null!;
    public DateTime? ActualDeparture { get; set; }
    public DateTime? ActualArrival { get; set; }

    // Navigation properties
    public Train Train { get; set; } = null!;
    public ICollection<TripFollower> Followers { get; set; } = new List<TripFollower>();
    public ICollection<TripLiveUpdate> LiveUpdates { get; set; } = new List<TripLiveUpdate>();
}
