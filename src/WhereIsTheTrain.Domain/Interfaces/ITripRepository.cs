using WhereIsTheTrain.Domain.Entities;

namespace WhereIsTheTrain.Domain.Interfaces;

public interface ITripRepository : IGenericRepository<Trip>
{
    Task<IReadOnlyList<Trip>> GetTodayTripsAsync(CancellationToken cancellationToken = default);
    Task<Trip?> GetWithDetailsAsync(Guid tripId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Trip>> GetFollowedTripsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Trip>> GetUserTripHistoryAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Trip?> GetByTrainAndDateAsync(Guid trainId, DateOnly date, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Trip>> GetTripsByTrainIdAsync(Guid trainId, CancellationToken cancellationToken = default);
}
