using WhereIsTheTrain.Domain.Common;

namespace WhereIsTheTrain.Domain.Entities;

public class SudokuSession : AuditableEntity
{
    public Guid UserId { get; set; }
    public string Difficulty { get; set; } = string.Empty; // "Easy", "Medium", "Hard"
    public int PuzzleIndex { get; set; } // 0-19
    public string CellState { get; set; } = string.Empty; // 81-character string of current progress
    public int ElapsedSeconds { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsDiscarded { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
}
