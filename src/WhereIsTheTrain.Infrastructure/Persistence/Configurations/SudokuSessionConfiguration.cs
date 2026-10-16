using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WhereIsTheTrain.Domain.Entities;

namespace WhereIsTheTrain.Infrastructure.Persistence.Configurations;

public class SudokuSessionConfiguration : IEntityTypeConfiguration<SudokuSession>
{
    public void Configure(EntityTypeBuilder<SudokuSession> builder)
    {
        builder.ToTable("SudokuSessions");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();
        
        builder.Property(s => s.Difficulty).HasMaxLength(20).IsRequired();
        builder.Property(s => s.CellState).HasMaxLength(200).IsRequired();

        // One-to-many relationship with User
        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Fast lookup index for active session
        builder.HasIndex(s => new { s.UserId, s.IsCompleted, s.IsDiscarded });
    }
}
