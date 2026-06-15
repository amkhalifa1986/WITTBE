using WhereIsTheTrain.Domain.Entities;

namespace WhereIsTheTrain.Domain.Interfaces;

public interface ITrainRepository : IGenericRepository<Train>
{
    Task<Train?> GetByTrainNumberAsync(string trainNumber, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Train>> SearchByNumberAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Train>> SearchByStopsAsync(string fromStop, string toStop, CancellationToken cancellationToken = default);
    Task<Train?> GetWithRouteAsync(Guid trainId, CancellationToken cancellationToken = default);
}
