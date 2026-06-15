using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WhereIsTheTrain.Domain.Entities;

namespace WhereIsTheTrain.Infrastructure.Persistence.Configurations;

public class TrainConfiguration : IEntityTypeConfiguration<Train>
{
    public void Configure(EntityTypeBuilder<Train> builder)
    {
        builder.ToTable("Trains");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();
        builder.Property(t => t.TrainNumber).HasMaxLength(50).IsRequired();
        builder.HasIndex(t => t.TrainNumber).IsUnique();
        builder.Property(t => t.NameAr).HasMaxLength(200).IsRequired();
        builder.Property(t => t.NameEn).HasMaxLength(200).IsRequired();
        builder.Property(t => t.DescriptionAr).HasMaxLength(1000);
        builder.Property(t => t.DescriptionEn).HasMaxLength(1000);

        builder.HasOne(t => t.CreatedBy)
            .WithMany()
            .HasForeignKey(t => t.CreatedById)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.TrainType)
            .WithMany(tt => tt.Trains)
            .HasForeignKey(t => t.TrainTypeId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
