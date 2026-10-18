using Microsoft.EntityFrameworkCore;
using WhereIsTheTrain.Domain.Entities;
using WhereIsTheTrain.Domain.Interfaces;

namespace WhereIsTheTrain.Infrastructure.Persistence.Repositories;

public class StopRepository : GenericRepository<Stop>, IStopRepository
{
    public StopRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Stop?> GetWithRailwayPathsByIdAsync(Guid id, CancellationToken ct = default)
        => await _dbSet
            .Include(s => s.RailwayPaths)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<IReadOnlyList<Stop>> GetAllWithRailwayPathsAsync(CancellationToken ct = default)
        => await _dbSet
            .Include(s => s.RailwayPaths)
            .ToListAsync(ct);

    /// <summary>
    /// Directly manages the RailwayPathStops join table via raw SQL.
    /// EF Core's shadow join table change tracking is unreliable when entities
    /// are loaded in separate queries, so this bypasses it entirely.
    /// </summary>
    public async Task SetRailwayPathsAsync(Guid stopId, List<Guid> railwayPathIds, CancellationToken ct = default)
    {
        // Delete all existing associations for this stop
        await _context.Database.ExecuteSqlRawAsync(
            "DELETE FROM RailwayPathStops WHERE StopsId = {0}",
            new object[] { stopId.ToString() },
            ct);

        // Insert new associations
        foreach (var pathId in railwayPathIds)
        {
            await _context.Database.ExecuteSqlRawAsync(
                "INSERT IGNORE INTO RailwayPathStops (RailwayPathsId, StopsId) VALUES ({0}, {1})",
                new object[] { pathId.ToString(), stopId.ToString() },
                ct);
        }
    }
}
