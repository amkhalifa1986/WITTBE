using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WhereIsTheTrain.Domain.Entities;

namespace WhereIsTheTrain.Infrastructure.Persistence.Configurations;

public class StopConfiguration : IEntityTypeConfiguration<Stop>
{
    public void Configure(EntityTypeBuilder<Stop> builder)
    {
        builder.ToTable("Stops");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();
        builder.Property(s => s.NameAr).IsRequired().HasMaxLength(100);
        builder.Property(s => s.NameEn).IsRequired().HasMaxLength(100);
        builder.Property(s => s.Code).HasMaxLength(20).IsRequired();
        builder.HasIndex(s => s.Code).IsUnique();
        builder.HasOne(s => s.City)
            .WithMany(c => c.Stops)
            .HasForeignKey(s => s.CityId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(s => s.DescriptionAr).HasMaxLength(500);
        builder.Property(s => s.DescriptionEn).HasMaxLength(500);
        builder.Property(s => s.Location).HasColumnType("geometry");

        builder.HasMany(s => s.RailwayPaths)
            .WithMany(rp => rp.Stops)
            .UsingEntity(j => j.ToTable("RailwayPathStops"));
    }
}
