using WhereIsTheTrain.Domain.Common;
using WhereIsTheTrain.Domain.Enums;

namespace WhereIsTheTrain.Domain.Entities;

public class User : AuditableEntity
{
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public UserRole Role { get; set; } = UserRole.User;
    public bool EmailConfirmed { get; set; } = false;
    public bool IsSuspended { get; set; } = false;
    public string? EmailConfirmationToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }

    // Navigation properties
    public ICollection<TripFollower> FollowedTrips { get; set; } = new List<TripFollower>();
    public ICollection<TrainFollowPlan> FollowPlans { get; set; } = new List<TrainFollowPlan>();
    public ICollection<TripLiveUpdate> LiveUpdates { get; set; } = new List<TripLiveUpdate>();
    public ICollection<LostFoundPost> LostFoundPosts { get; set; } = new List<LostFoundPost>();
    public ICollection<LostFoundComment> LostFoundComments { get; set; } = new List<LostFoundComment>();
    public ICollection<TrainSuggestion> TrainSuggestions { get; set; } = new List<TrainSuggestion>();
    public ICollection<DeviceToken> DeviceTokens { get; set; } = new List<DeviceToken>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<TripLiveUpdateThanks> ThanksList { get; set; } = new List<TripLiveUpdateThanks>();
}

