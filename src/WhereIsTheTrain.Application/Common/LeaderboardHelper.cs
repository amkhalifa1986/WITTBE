using System.Linq.Expressions;
using WhereIsTheTrain.Application.Features.Points;
using WhereIsTheTrain.Domain.Entities;
using WhereIsTheTrain.Domain.Interfaces;

namespace WhereIsTheTrain.Application.Common;

public static class LeaderboardHelper
{
    /// <summary>
    /// Top users by points for the logs matching <paramref name="filter"/>.
    /// Totals are grouped and ranked in the database, and the users are loaded in one query,
    /// instead of loading every point log and then each user one by one.
    /// </summary>
    public static async Task<List<LeaderboardEntryDto>> GetTopAsync(
        IUnitOfWork unitOfWork,
        Expression<Func<UserPointLog, bool>> filter,
        int top,
        CancellationToken ct)
    {
        var totals = await unitOfWork.Repository<UserPointLog>().QueryAsync(q => q
            .Where(filter)
            .GroupBy(l => l.UserId)
            .Select(g => new { UserId = g.Key, Total = g.Sum(l => l.PointsAwarded) })
            .OrderByDescending(x => x.Total)
            .Take(top), ct);

        if (totals.Count == 0)
            return new List<LeaderboardEntryDto>();

        var userIds = totals.Select(t => t.UserId).ToList();
        var users = (await unitOfWork.Users.FindAsync(u => userIds.Contains(u.Id), ct))
            .ToDictionary(u => u.Id);

        var result = new List<LeaderboardEntryDto>();
        int rank = 1;
        foreach (var entry in totals)
        {
            if (users.TryGetValue(entry.UserId, out var user))
            {
                result.Add(new LeaderboardEntryDto
                {
                    UserId = entry.UserId,
                    DisplayName = user.DisplayName,
                    AvatarUrl = user.AvatarUrl,
                    TotalPoints = entry.Total,
                    Rank = rank++
                });
            }
        }
        return result;
    }
}
