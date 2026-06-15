using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WhereIsTheTrain.Domain.Entities;

namespace WhereIsTheTrain.Infrastructure.Persistence.Configurations;

public class RailwayPathConfiguration : IEntityTypeConfiguration<RailwayPath>
{
    public void Configure(EntityTypeBuilder<RailwayPath> builder)
    {
        builder.ToTable("RailwayPaths");
        builder.HasKey(rp => rp.Id);
        builder.Property(rp => rp.Id).ValueGeneratedOnAdd();
        builder.Property(rp => rp.Code).HasMaxLength(50).IsRequired();
        builder.Property(rp => rp.NameAr).HasMaxLength(200).IsRequired();
        builder.Property(rp => rp.NameEn).HasMaxLength(200).IsRequired();
        builder.Property(rp => rp.RoutePath).HasColumnType("geometry").IsRequired();

        builder.HasIndex(rp => rp.Code).IsUnique();

        // Foreign keys to Stop entity (StartStation and EndStation)
        // Set DeleteBehavior.Restrict to prevent deleting a station from cascading to path deletion
        builder.HasOne(rp => rp.StartStation)
            .WithMany()
            .HasForeignKey(rp => rp.StartStationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(rp => rp.EndStation)
            .WithMany()
            .HasForeignKey(rp => rp.EndStationId)
            .OnDelete(DeleteBehavior.Restrict);

        // Prevent duplicate paths for the exact same station connection
        builder.HasIndex(rp => new { rp.StartStationId, rp.EndStationId }).IsUnique();
    }
}
