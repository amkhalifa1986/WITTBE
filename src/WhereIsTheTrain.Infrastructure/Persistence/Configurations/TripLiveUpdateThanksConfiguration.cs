using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WhereIsTheTrain.Domain.Entities;

namespace WhereIsTheTrain.Infrastructure.Persistence.Configurations;

public class TripLiveUpdateThanksConfiguration : IEntityTypeConfiguration<TripLiveUpdateThanks>
{
    public void Configure(EntityTypeBuilder<TripLiveUpdateThanks> builder)
    {
        builder.ToTable("TripLiveUpdateThanks");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();

        // Unique index to prevent duplicate thanks from the same user for the same report
        builder.HasIndex(t => new { t.TripLiveUpdateId, t.UserId }).IsUnique();

        builder.HasOne(t => t.TripLiveUpdate)
            .WithMany(u => u.ThanksList)
            .HasForeignKey(t => t.TripLiveUpdateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.User)
            .WithMany(u => u.ThanksList)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
