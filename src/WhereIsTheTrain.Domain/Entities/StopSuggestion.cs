using WhereIsTheTrain.Domain.Common;
using WhereIsTheTrain.Domain.Enums;

namespace WhereIsTheTrain.Domain.Entities;

public class StopSuggestion : AuditableEntity
{
    public Guid SuggestedById { get; set; }
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    
    // Existing city mapping
    public Guid? CityId { get; set; }
    
    // Inline new city suggestion fields
    public string? NewCityNameAr { get; set; }
    public string? NewCityNameEn { get; set; }
    public Guid? NewCityGovernorateId { get; set; }
    
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }
    
    public SuggestionStatus Status { get; set; } = SuggestionStatus.Pending;
    public string? AdminNotes { get; set; }

    // Navigation properties
    public User SuggestedBy { get; set; } = null!;
    public City? City { get; set; }
    public Governorate? NewCityGovernorate { get; set; }
}
