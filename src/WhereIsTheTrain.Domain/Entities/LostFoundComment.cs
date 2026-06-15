using WhereIsTheTrain.Domain.Common;

namespace WhereIsTheTrain.Domain.Entities;

public class LostFoundComment : AuditableEntity
{
    public Guid PostId { get; set; }
    public Guid AuthorId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsHidden { get; set; } = false;

    // Navigation properties
    public LostFoundPost Post { get; set; } = null!;
    public User Author { get; set; } = null!;
}
