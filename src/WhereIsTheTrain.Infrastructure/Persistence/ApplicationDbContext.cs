using Microsoft.EntityFrameworkCore;
using WhereIsTheTrain.Domain.Entities;

namespace WhereIsTheTrain.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Train> Trains => Set<Train>();
    public DbSet<TrainType> TrainTypes => Set<TrainType>();
    public DbSet<Stop> Stops => Set<Stop>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<Governorate> Governorates => Set<Governorate>();
    public DbSet<TrainRouteStop> TrainRouteStops => Set<TrainRouteStop>();
    public DbSet<Trip> Trips => Set<Trip>();
    public DbSet<TripFollower> TripFollowers => Set<TripFollower>();
    public DbSet<TrainFollowPlan> TrainFollowPlans => Set<TrainFollowPlan>();
    public DbSet<TripLiveUpdate> TripLiveUpdates => Set<TripLiveUpdate>();
    public DbSet<LostFoundPost> LostFoundPosts => Set<LostFoundPost>();
    public DbSet<LostFoundComment> LostFoundComments => Set<LostFoundComment>();
    public DbSet<TrainSuggestion> TrainSuggestions => Set<TrainSuggestion>();
    public DbSet<StopSuggestion> StopSuggestions => Set<StopSuggestion>();
    public DbSet<TripTelemetry> TripTelemetries => Set<TripTelemetry>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<TripLiveUpdateThanks> TripLiveUpdateThanks => Set<TripLiveUpdateThanks>();
    public DbSet<RailwayPath> RailwayPaths => Set<RailwayPath>();
    public DbSet<AdImpression> AdImpressions => Set<AdImpression>();
    public DbSet<AdClick> AdClicks => Set<AdClick>();

    public DbSet<DeviceToken> DeviceTokens => Set<DeviceToken>();
    public DbSet<ServiceDisruption> ServiceDisruptions => Set<ServiceDisruption>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
    public DbSet<StatusTagLookup> StatusTagLookups => Set<StatusTagLookup>();
    public DbSet<CrowdLevelLookup> CrowdLevelLookups => Set<CrowdLevelLookup>();

    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
    public DbSet<AdminRole> AdminRoles => Set<AdminRole>();
    public DbSet<AdminRolePrivilege> AdminRolePrivileges => Set<AdminRolePrivilege>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<Domain.Common.AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
