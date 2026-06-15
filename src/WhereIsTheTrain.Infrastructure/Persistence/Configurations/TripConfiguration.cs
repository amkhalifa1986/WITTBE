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

        builder.HasOne(t => t.Train)
            .WithMany(tr => tr.Trips)
            .HasForeignKey(t => t.TrainId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
