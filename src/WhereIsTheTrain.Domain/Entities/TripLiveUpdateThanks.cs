using WhereIsTheTrain.Domain.Common;

namespace WhereIsTheTrain.Domain.Entities;

public class TripLiveUpdateThanks : BaseEntity
{
    public Guid TripLiveUpdateId { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public TripLiveUpdate TripLiveUpdate { get; set; } = null!;
    public User User { get; set; } = null!;
}
