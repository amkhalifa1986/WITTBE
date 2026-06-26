using WhereIsTheTrain.Domain.Common;

namespace WhereIsTheTrain.Domain.Entities;

public class SystemSetting : BaseEntity
{
    public bool LostFoundPostAutoPublish { get; set; } = true;
    public bool LostFoundCommentAutoPublish { get; set; } = true;
    public bool TripLiveUpdateAutoPublish { get; set; } = true;
    /// <summary>
    /// When true, a user's removal request is immediately applied (post deleted).
    /// When false, the request is queued for admin review.
    /// </summary>
    public bool TripLiveUpdateRemovalAutoApprove { get; set; } = false;
    public string AdsEnabledPages { get; set; } = "{}";
    public bool GpsTrackingEnabled { get; set; } = true;
}
