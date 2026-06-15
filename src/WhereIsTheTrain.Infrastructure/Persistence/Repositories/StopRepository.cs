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
}
