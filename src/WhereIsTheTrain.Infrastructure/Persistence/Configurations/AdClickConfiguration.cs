using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WhereIsTheTrain.Domain.Entities;

namespace WhereIsTheTrain.Infrastructure.Persistence.Configurations;

public class AdClickConfiguration : IEntityTypeConfiguration<AdClick>
{
    public void Configure(EntityTypeBuilder<AdClick> builder)
    {
        builder.ToTable("AdClicks");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.ScreenId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.VisitorId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Timestamp)
            .IsRequired();

        builder.Property(x => x.TrainNumber)
            .HasMaxLength(50);

        // Configure indexes for high performance aggregations
        builder.HasIndex(x => x.Timestamp);
        builder.HasIndex(x => x.ScreenId);
        builder.HasIndex(x => x.TrainNumber);
        builder.HasIndex(x => new { x.Timestamp, x.ScreenId });

        // Configure relation with User
        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
