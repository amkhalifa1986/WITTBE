using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WhereIsTheTrain.Domain.Entities;

namespace WhereIsTheTrain.Infrastructure.Persistence.Configurations;

public class TripFollowerConfiguration : IEntityTypeConfiguration<TripFollower>
{
    public void Configure(EntityTypeBuilder<TripFollower> builder)
    {
        builder.ToTable("TripFollowers");
        builder.HasKey(tf => tf.Id);
        builder.Property(tf => tf.Id).ValueGeneratedNever();
        builder.HasIndex(tf => new { tf.UserId, tf.TripId }).IsUnique();

        builder.HasOne(tf => tf.User)
            .WithMany(u => u.FollowedTrips)
            .HasForeignKey(tf => tf.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(tf => tf.Trip)
            .WithMany(t => t.Followers)
            .HasForeignKey(tf => tf.TripId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(tf => tf.SourcePlan)
            .WithMany()
            .HasForeignKey(tf => tf.SourcePlanId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
