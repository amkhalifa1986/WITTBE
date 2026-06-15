namespace WhereIsTheTrain.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IAdminUserRepository AdminUsers { get; }
    IAdminRoleRepository AdminRoles { get; }
    ITrainRepository Trains { get; }
    ITripRepository Trips { get; }
    IRailwayPathRepository RailwayPaths { get; }
    IStopRepository Stops { get; }
    IGenericRepository<T> Repository<T>() where T : Common.BaseEntity;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
