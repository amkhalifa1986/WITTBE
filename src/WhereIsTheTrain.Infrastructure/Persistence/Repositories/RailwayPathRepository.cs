using Microsoft.EntityFrameworkCore;
using WhereIsTheTrain.Domain.Entities;
using WhereIsTheTrain.Domain.Interfaces;

namespace WhereIsTheTrain.Infrastructure.Persistence.Repositories;

public class RailwayPathRepository : GenericRepository<RailwayPath>, IRailwayPathRepository
{
    public RailwayPathRepository(ApplicationDbContext context) : base(context) { }

    public async Task<RailwayPath?> GetWithStationsByIdAsync(Guid id, CancellationToken ct = default)
        => await _dbSet
            .Include(rp => rp.StartStation)
            .Include(rp => rp.EndStation)
            .FirstOrDefaultAsync(rp => rp.Id == id, ct);

    public async Task<IReadOnlyList<RailwayPath>> GetAllWithStationsAsync(CancellationToken ct = default)
        => await _dbSet
            .Include(rp => rp.StartStation)
            .Include(rp => rp.EndStation)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<RailwayPath>> GetAllWithStopsAsync(CancellationToken ct = default)
        => await _dbSet
            .Include(rp => rp.Stops)
            .ToListAsync(ct);
}
