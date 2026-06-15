using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WhereIsTheTrain.Domain.Entities;

namespace WhereIsTheTrain.Infrastructure.Persistence.Configurations;

public class TripLiveUpdateConfiguration : IEntityTypeConfiguration<TripLiveUpdate>
{
    public void Configure(EntityTypeBuilder<TripLiveUpdate> builder)
    {
        builder.ToTable("TripLiveUpdates");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).ValueGeneratedNever();
        builder.Property(u => u.AuthorId).IsRequired(false);
        builder.Property(u => u.Content).HasMaxLength(1000).IsRequired();
        builder.Property(u => u.StatusTag).HasMaxLength(50);
        builder.Property(u => u.CrowdState).HasMaxLength(50).IsRequired(false);
        builder.Property(u => u.IsApproved).HasDefaultValue(true);
        builder.Property(u => u.IsRemovalRequested).HasDefaultValue(false);
        builder.HasIndex(u => new { u.TripId, u.CreatedAt });

        builder.HasOne(u => u.Trip)
            .WithMany(t => t.LiveUpdates)
            .HasForeignKey(u => u.TripId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.Author)
            .WithMany(a => a.LiveUpdates)
            .HasForeignKey(u => u.AuthorId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
