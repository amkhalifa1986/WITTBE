using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WhereIsTheTrain.Domain.Entities;

namespace WhereIsTheTrain.Infrastructure.Persistence.Configurations;

public class TripConfiguration : IEntityTypeConfiguration<Trip>
{
    public void Configure(EntityTypeBuilder<Trip> builder)
    {
        builder.ToTable("Trips");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();
        builder.HasIndex(t => new { t.TrainId, t.TripDate }).IsUnique();
        // Index on TripDate — GetTodayTrips is called on every dashboard load
        builder.HasIndex(t => t.TripDate)
            .HasDatabaseName("IX_Trips_TripDate");
        // Index on StatusId — used by dashboard stats count queries
        builder.HasIndex(t => t.StatusId)
            .HasDatabaseName("IX_Trips_StatusId");

        builder.HasOne(t => t.Train)
            .WithMany(tr => tr.Trips)
            .HasForeignKey(t => t.TrainId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Status)
            .WithMany()
            .HasForeignKey(t => t.StatusId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
