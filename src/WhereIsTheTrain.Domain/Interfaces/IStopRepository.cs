using WhereIsTheTrain.Domain.Entities;

namespace WhereIsTheTrain.Domain.Interfaces;

public interface IStopRepository : IGenericRepository<Stop>
{
    Task<Stop?> GetWithRailwayPathsByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Stop>> GetAllWithRailwayPathsAsync(CancellationToken ct = default);
}
