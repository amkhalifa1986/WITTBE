using WhereIsTheTrain.Domain.Common;

namespace WhereIsTheTrain.Domain.Entities;

public class SudokuScore : AuditableEntity
{
    public Guid UserId { get; set; }
    public string Difficulty { get; set; } = string.Empty; // "Easy", "Medium", "Hard"
    public int TimeSeconds { get; set; }
    public bool IsPersonalBest { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
}
