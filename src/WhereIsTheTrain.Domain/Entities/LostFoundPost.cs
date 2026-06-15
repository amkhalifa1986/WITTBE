using WhereIsTheTrain.Domain.Common;
using WhereIsTheTrain.Domain.Enums;

namespace WhereIsTheTrain.Domain.Entities;

public class LostFoundPost : AuditableEntity
{
    public Guid AuthorId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public LostFoundType Type { get; set; }
    public string? TrainNumber { get; set; }
    public string? ContactInfo { get; set; }
    public bool IsResolved { get; set; } = false;
    public LostFoundStatus Status { get; set; } = LostFoundStatus.New;

    // Navigation properties
    public User Author { get; set; } = null!;
    public ICollection<LostFoundComment> Comments { get; set; } = new List<LostFoundComment>();
}

