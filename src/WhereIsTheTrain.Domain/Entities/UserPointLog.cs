using System;

namespace WhereIsTheTrain.Domain.Entities;

public class UserPointLog : Common.AuditableEntity
{
    public Guid UserId { get; set; }
    public Guid PointActionId { get; set; }
    public Guid? TripId { get; set; }
    public int PointsAwarded { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public PointAction Action { get; set; } = null!;
    public Trip? Trip { get; set; }
}
