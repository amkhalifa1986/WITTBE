using System;
using System.Collections.Generic;

namespace WhereIsTheTrain.Application.Features.Points;

public class PointActionDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public int Points { get; set; }
}

public class LeaderboardEntryDto
{
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public int TotalPoints { get; set; }
    public int Rank { get; set; }
}

public class UserPointLogDto
{
    public Guid Id { get; set; }
    public string ActionCode { get; set; } = string.Empty;
    public string ActionNameEn { get; set; } = string.Empty;
    public string ActionNameAr { get; set; } = string.Empty;
    public int PointsAwarded { get; set; }
    public Guid? TripId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UserPointsSummaryDto
{
    public int TotalPoints { get; set; }
    public List<UserPointLogDto> Logs { get; set; } = new();
}
