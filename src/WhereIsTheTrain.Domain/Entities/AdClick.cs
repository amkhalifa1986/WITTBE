using WhereIsTheTrain.Domain.Common;

namespace WhereIsTheTrain.Domain.Entities;

public class AdClick : BaseEntity
{
    public string ScreenId { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public string VisitorId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? TrainNumber { get; set; }

    // Navigation property
    public User? User { get; set; }
}
