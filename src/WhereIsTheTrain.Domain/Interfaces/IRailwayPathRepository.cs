using WhereIsTheTrain.Domain.Entities;

namespace WhereIsTheTrain.Domain.Interfaces;

public interface IRailwayPathRepository : IGenericRepository<RailwayPath>
{
    Task<RailwayPath?> GetWithStationsByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<RailwayPath>> GetAllWithStationsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<RailwayPath>> GetAllWithStopsAsync(CancellationToken ct = default);
}
