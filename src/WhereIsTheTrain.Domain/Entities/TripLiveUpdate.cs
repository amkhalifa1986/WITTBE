using WhereIsTheTrain.Domain.Common;
using WhereIsTheTrain.Domain.Enums;

namespace WhereIsTheTrain.Domain.Entities;

public class TripLiveUpdate : AuditableEntity
{
    public Guid TripId { get; set; }
    public Guid? AuthorId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? StatusTag { get; set; }
    public string? CrowdState { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool IsApproved { get; set; } = true;
    public bool IsRemovalRequested { get; set; } = false;

    // Navigation properties
    public Trip Trip { get; set; } = null!;
    public User? Author { get; set; }
    public ICollection<TripLiveUpdateThanks> ThanksList { get; set; } = new List<TripLiveUpdateThanks>();
}
