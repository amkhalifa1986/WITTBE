using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WhereIsTheTrain.Domain.Entities;

namespace WhereIsTheTrain.Infrastructure.Persistence.Configurations;

public class TrainFollowPlanConfiguration : IEntityTypeConfiguration<TrainFollowPlan>
{
    public void Configure(EntityTypeBuilder<TrainFollowPlan> builder)
    {
        builder.ToTable("TrainFollowPlans");
        builder.HasKey(fp => fp.Id);
        builder.Property(fp => fp.Id).ValueGeneratedNever();

        builder.HasIndex(fp => new { fp.UserId, fp.TrainId, fp.DayOfWeek }).IsUnique();

        builder.HasOne(fp => fp.User)
            .WithMany(u => u.FollowPlans)
            .HasForeignKey(fp => fp.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(fp => fp.Train)
            .WithMany()
            .HasForeignKey(fp => fp.TrainId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(fp => fp.TargetStop)
            .WithMany()
            .HasForeignKey(fp => fp.TargetStopId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
