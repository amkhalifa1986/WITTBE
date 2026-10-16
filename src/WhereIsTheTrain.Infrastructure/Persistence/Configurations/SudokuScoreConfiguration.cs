using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WhereIsTheTrain.Domain.Entities;

namespace WhereIsTheTrain.Infrastructure.Persistence.Configurations;

public class SudokuScoreConfiguration : IEntityTypeConfiguration<SudokuScore>
{
    public void Configure(EntityTypeBuilder<SudokuScore> builder)
    {
        builder.ToTable("SudokuScores");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.Difficulty).HasMaxLength(20).IsRequired();

        // One-to-many relationship with User
        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index for leaderboard queries
        builder.HasIndex(s => new { s.Difficulty, s.TimeSeconds });
        // Index for personal best check
        builder.HasIndex(s => new { s.UserId, s.Difficulty });
    }
}
