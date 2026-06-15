using WhereIsTheTrain.Domain.Common;
using WhereIsTheTrain.Domain.Enums;

namespace WhereIsTheTrain.Domain.Entities;

public class TrainSuggestion : AuditableEntity
{
    public Guid SuggestedById { get; set; }
    public string TrainNumber { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }
    public string? RouteDescriptionAr { get; set; }
    public string? RouteDescriptionEn { get; set; }
    public SuggestionStatus Status { get; set; } = SuggestionStatus.Pending;
    public string? AdminNotes { get; set; }

    // Navigation properties
    public User SuggestedBy { get; set; } = null!;
}
