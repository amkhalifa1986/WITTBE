using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WhereIsTheTrain.Domain.Entities;

namespace WhereIsTheTrain.Infrastructure.Persistence.Configurations;

public class ServiceDisruptionConfiguration : IEntityTypeConfiguration<ServiceDisruption>
{
    public void Configure(EntityTypeBuilder<ServiceDisruption> builder)
    {
        builder.ToTable("ServiceDisruptions");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();
        builder.Property(s => s.TitleAr).HasMaxLength(200).IsRequired();
        builder.Property(s => s.TitleEn).HasMaxLength(200).IsRequired();
        builder.Property(s => s.DescriptionAr).HasMaxLength(1000).IsRequired();
        builder.Property(s => s.DescriptionEn).HasMaxLength(1000).IsRequired();
        builder.Property(s => s.AffectedLine).HasMaxLength(100);
    }
}
