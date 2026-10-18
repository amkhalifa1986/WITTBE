using System;

namespace WhereIsTheTrain.Application.Features.Sudoku;

public class SudokuSessionDto
{
    public Guid Id { get; set; }
    public string Difficulty { get; set; } = string.Empty;
    public int PuzzleIndex { get; set; }
    public string CellState { get; set; } = string.Empty;
    public int ElapsedSeconds { get; set; }
}

public class SudokuLeaderboardEntryDto
{
    public int Rank { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public int TimeSeconds { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SudokuMyBestDto
{
    public string Difficulty { get; set; } = string.Empty;
    public int BestTimeSeconds { get; set; }
}
