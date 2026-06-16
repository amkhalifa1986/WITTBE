using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WhereIsTheTrain.Domain.Entities;

namespace WhereIsTheTrain.Infrastructure.Persistence.Configurations;

public class StopSuggestionConfiguration : IEntityTypeConfiguration<StopSuggestion>
{
    public void Configure(EntityTypeBuilder<StopSuggestion> builder)
    {
        builder.ToTable("StopSuggestions");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();
        builder.Property(s => s.Code).HasMaxLength(50).IsRequired();
        builder.Property(s => s.NameAr).HasMaxLength(200).IsRequired();
        builder.Property(s => s.NameEn).HasMaxLength(200).IsRequired();
        builder.Property(s => s.NewCityNameAr).HasMaxLength(200);
        builder.Property(s => s.NewCityNameEn).HasMaxLength(200);
        builder.Property(s => s.DescriptionAr).HasMaxLength(1000);
        builder.Property(s => s.DescriptionEn).HasMaxLength(1000);
        builder.Property(s => s.AdminNotes).HasMaxLength(1000);

        builder.HasOne(s => s.SuggestedBy)
            .WithMany(u => u.StopSuggestions)
            .HasForeignKey(s => s.SuggestedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.City)
            .WithMany()
            .HasForeignKey(s => s.CityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.NewCityGovernorate)
            .WithMany()
            .HasForeignKey(s => s.NewCityGovernorateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
