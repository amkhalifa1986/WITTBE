using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WhereIsTheTrain.Domain.Entities;

namespace WhereIsTheTrain.Infrastructure.Persistence.Configurations;

public class TripStatusLookupConfiguration : IEntityTypeConfiguration<TripStatusLookup>
{
    public void Configure(EntityTypeBuilder<TripStatusLookup> builder)
    {
        builder.ToTable("TripStatusLookups");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).ValueGeneratedNever();
        builder.Property(l => l.Code).HasMaxLength(50).IsRequired();
        builder.Property(l => l.NameAr).HasMaxLength(100).IsRequired();
        builder.Property(l => l.NameEn).HasMaxLength(100).IsRequired();
        builder.Property(l => l.Color).HasMaxLength(20).IsRequired();

        builder.HasIndex(l => l.Code).IsUnique();
    }
}
