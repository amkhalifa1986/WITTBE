using WhereIsTheTrain.Domain.Common;
using WhereIsTheTrain.Domain.Enums;

namespace WhereIsTheTrain.Domain.Entities;

public class TripFollower : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid TripId { get; set; }
    public PersonalTripStatus PersonalStatus { get; set; } = PersonalTripStatus.Following;
    public DateTime FollowedAt { get; set; } = DateTime.UtcNow;
    public Guid? SourcePlanId { get; set; }
    public bool NotificationsEnabled { get; set; } = true;

    // Navigation properties
    public User User { get; set; } = null!;
    public Trip Trip { get; set; } = null!;
    public TrainFollowPlan? SourcePlan { get; set; }
}
