using Microsoft.EntityFrameworkCore;
using WhereIsTheTrain.Domain.Entities;
using WhereIsTheTrain.Domain.Enums;
using WhereIsTheTrain.Domain.Interfaces;

namespace WhereIsTheTrain.Infrastructure.Persistence.Repositories;

public class TripRepository : GenericRepository<Trip>, ITripRepository
{
    public TripRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Trip>> GetTodayTripsAsync(CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return await _dbSet
            .Include(t => t.Status)
            .Include(t => t.Train)
                .ThenInclude(tr => tr.TrainType)
            .Include(t => t.Train)
                .ThenInclude(tr => tr.RouteStops.OrderBy(rs => rs.StopOrder))
                    .ThenInclude(rs => rs.Stop)
            .Where(t => t.TripDate == today)
            .OrderBy(t => t.Train.RouteStops
                .OrderBy(rs => rs.StopOrder)
                .Select(rs => rs.ScheduledDeparture)
                .FirstOrDefault())
            .ToListAsync(cancellationToken);
    }

    public async Task<Trip?> GetWithDetailsAsync(Guid tripId, CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(t => t.Status)
            .Include(t => t.Train)
                .ThenInclude(tr => tr.TrainType)
            .Include(t => t.Train)
                .ThenInclude(tr => tr.RouteStops.OrderBy(rs => rs.StopOrder))
                    .ThenInclude(rs => rs.Stop)
                        .ThenInclude(s => s.City)
            .Include(t => t.LiveUpdates.OrderByDescending(u => u.CreatedAt))
                .ThenInclude(u => u.Author)
            .Include(t => t.LiveUpdates)
                .ThenInclude(u => u.ThanksList)
            .Include(t => t.Followers)
            .FirstOrDefaultAsync(t => t.Id == tripId, cancellationToken);

    public async Task<IReadOnlyList<Trip>> GetFollowedTripsAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(t => t.Status)
            .Include(t => t.Train)
                .ThenInclude(tr => tr.TrainType)
            .Include(t => t.Followers)
            .Where(t => t.Followers.Any(f => f.UserId == userId))
            .OrderByDescending(t => t.TripDate)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Trip>> GetUserTripHistoryAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(t => t.Status)
            .Include(t => t.Train)
                .ThenInclude(tr => tr.TrainType)
            .Include(t => t.Followers)
            .Where(t => t.Followers.Any(f => f.UserId == userId && f.PersonalStatus == PersonalTripStatus.Ended))
            .OrderByDescending(t => t.TripDate)
            .ToListAsync(cancellationToken);

    public async Task<Trip?> GetByTrainAndDateAsync(Guid trainId, DateOnly date, CancellationToken cancellationToken = default)
        => await _dbSet.FirstOrDefaultAsync(t => t.TrainId == trainId && t.TripDate == date, cancellationToken);

    public async Task<IReadOnlyList<Trip>> GetTripsByTrainIdAsync(Guid trainId, CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(t => t.Status)
            .Include(t => t.Train)
            .Include(t => t.Followers)
            .Where(t => t.TrainId == trainId)
            .OrderByDescending(t => t.TripDate)
            .ToListAsync(cancellationToken);
}
