using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WhereIsTheTrain.Domain.Entities;

namespace WhereIsTheTrain.Infrastructure.Persistence.Configurations;

public class LostFoundPostConfiguration : IEntityTypeConfiguration<LostFoundPost>
{
    public void Configure(EntityTypeBuilder<LostFoundPost> builder)
    {
        builder.ToTable("LostFoundPosts");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();
        builder.Property(p => p.Title).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(2000).IsRequired();
        builder.Property(p => p.ImageUrl).HasMaxLength(500);
        builder.Property(p => p.TrainNumber).HasMaxLength(50);
        builder.Property(p => p.ContactInfo).HasMaxLength(255);

        builder.HasOne(p => p.Author)
            .WithMany(u => u.LostFoundPosts)
            .HasForeignKey(p => p.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
