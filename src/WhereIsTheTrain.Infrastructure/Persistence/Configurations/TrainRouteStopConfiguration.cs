using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WhereIsTheTrain.Domain.Entities;

namespace WhereIsTheTrain.Infrastructure.Persistence.Configurations;

public class TrainRouteStopConfiguration : IEntityTypeConfiguration<TrainRouteStop>
{
    public void Configure(EntityTypeBuilder<TrainRouteStop> builder)
    {
        builder.ToTable("TrainRouteStops");
        builder.HasKey(trs => trs.Id);
        builder.Property(trs => trs.Id).ValueGeneratedNever();
        builder.HasIndex(trs => new { trs.TrainId, trs.StopOrder }).IsUnique();

        builder.HasOne(trs => trs.Train)
            .WithMany(t => t.RouteStops)
            .HasForeignKey(trs => trs.TrainId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(trs => trs.Stop)
            .WithMany(s => s.TrainRouteStops)
            .HasForeignKey(trs => trs.StopId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
