using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WhereIsTheTrain.Domain.Entities;

namespace WhereIsTheTrain.Infrastructure.Persistence.Configurations;

public class TripTelemetryConfiguration : IEntityTypeConfiguration<TripTelemetry>
{
    public void Configure(EntityTypeBuilder<TripTelemetry> builder)
    {
        builder.ToTable("TripTelemetry");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.RawLatitude).IsRequired();
        builder.Property(t => t.RawLongitude).IsRequired();
        builder.Property(t => t.SnappedLatitude).IsRequired();
        builder.Property(t => t.SnappedLongitude).IsRequired();
        builder.Property(t => t.Speed).IsRequired();
        builder.Property(t => t.DistanceAlongRoute).IsRequired();
        builder.Property(t => t.Timestamp).IsRequired();

        builder.HasOne(t => t.Trip)
            .WithMany()
            .HasForeignKey(t => t.TripId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
