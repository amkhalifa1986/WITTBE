using WhereIsTheTrain.Domain.Common;
using WhereIsTheTrain.Domain.Enums;

namespace WhereIsTheTrain.Domain.Entities;

public class TrainFollowPlan : AuditableEntity
{
    public Guid UserId { get; set; }
    public Guid TrainId { get; set; }
    public int DayOfWeek { get; set; } // 0 = Sunday, 1 = Monday, ..., 6 = Saturday
    public TrainFollowRole RoleType { get; set; } = TrainFollowRole.Follower;
    public Guid TargetStopId { get; set; }
    public int AlertLeadTimeMinutes { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Train Train { get; set; } = null!;
    public Stop TargetStop { get; set; } = null!;
}
