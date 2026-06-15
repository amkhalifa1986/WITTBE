using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WhereIsTheTrain.Domain.Entities;

namespace WhereIsTheTrain.Infrastructure.Persistence.Configurations;

public class TrainSuggestionConfiguration : IEntityTypeConfiguration<TrainSuggestion>
{
    public void Configure(EntityTypeBuilder<TrainSuggestion> builder)
    {
        builder.ToTable("TrainSuggestions");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();
        builder.Property(s => s.TrainNumber).HasMaxLength(50).IsRequired();
        builder.Property(s => s.NameAr).HasMaxLength(200).IsRequired();
        builder.Property(s => s.NameEn).HasMaxLength(200).IsRequired();
        builder.Property(s => s.DescriptionAr).HasMaxLength(1000);
        builder.Property(s => s.DescriptionEn).HasMaxLength(1000);
        builder.Property(s => s.RouteDescriptionAr).HasMaxLength(1000);
        builder.Property(s => s.RouteDescriptionEn).HasMaxLength(1000);
        builder.Property(s => s.AdminNotes).HasMaxLength(1000);

        builder.HasOne(s => s.SuggestedBy)
            .WithMany(u => u.TrainSuggestions)
            .HasForeignKey(s => s.SuggestedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
