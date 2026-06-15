using Microsoft.EntityFrameworkCore;
using WhereIsTheTrain.Domain.Entities;
using WhereIsTheTrain.Domain.Interfaces;

namespace WhereIsTheTrain.Infrastructure.Persistence.Repositories;

public class TrainRepository : GenericRepository<Train>, ITrainRepository
{
    public TrainRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Train?> GetByTrainNumberAsync(string trainNumber, CancellationToken cancellationToken = default)
        => await _dbSet.FirstOrDefaultAsync(t => t.TrainNumber == trainNumber, cancellationToken);

    public async Task<IReadOnlyList<Train>> SearchByNumberAsync(string searchTerm, CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(t => t.RouteStops)
                .ThenInclude(rs => rs.Stop)
                    .ThenInclude(s => s.City)
            .Where(t => t.TrainNumber.Contains(searchTerm) || t.NameAr.Contains(searchTerm) || t.NameEn.Contains(searchTerm))
            .Where(t => t.IsActive)
            .OrderBy(t => t.TrainNumber)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Train>> SearchByStopsAsync(string fromStop, string toStop, CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(t => t.RouteStops)
                .ThenInclude(rs => rs.Stop)
                    .ThenInclude(s => s.City)
            .Where(t => t.IsActive)
            .Where(t => t.RouteStops.Any(fromRs => 
                (fromRs.Stop.NameAr.Contains(fromStop) || fromRs.Stop.NameEn.Contains(fromStop) || fromRs.Stop.Code.Contains(fromStop)) &&
                t.RouteStops.Any(toRs => 
                    (toRs.Stop.NameAr.Contains(toStop) || toRs.Stop.NameEn.Contains(toStop) || toRs.Stop.Code.Contains(toStop)) &&
                    fromRs.StopOrder < toRs.StopOrder)))
            .ToListAsync(cancellationToken);

    public async Task<Train?> GetWithRouteAsync(Guid trainId, CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(t => t.RouteStops.OrderBy(rs => rs.StopOrder))
                .ThenInclude(rs => rs.Stop)
                    .ThenInclude(s => s.City)
            .FirstOrDefaultAsync(t => t.Id == trainId, cancellationToken);
}
