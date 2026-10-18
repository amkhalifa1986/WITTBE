using WhereIsTheTrain.Domain.Common;

namespace WhereIsTheTrain.Domain.Entities;

public class AdminNotification : BaseEntity
{
    public Guid AdminUserId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string MessageAr { get; set; } = string.Empty;
    public string? Link { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public AdminUser AdminUser { get; set; } = null!;
}
