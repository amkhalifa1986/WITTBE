using WhereIsTheTrain.Domain.Common;

namespace WhereIsTheTrain.Domain.Entities;

public class DashboardGalleryItem : BaseEntity
{
    public string ImagePath { get; set; } = string.Empty;
    public string CaptionAr { get; set; } = string.Empty;
    public string CaptionEn { get; set; } = string.Empty;
    public bool IsVisible { get; set; } = true;
    public string? Link { get; set; }
}
