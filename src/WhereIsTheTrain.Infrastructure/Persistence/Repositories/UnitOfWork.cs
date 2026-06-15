using System.Collections;
using WhereIsTheTrain.Domain.Common;
using WhereIsTheTrain.Domain.Interfaces;

namespace WhereIsTheTrain.Infrastructure.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly Hashtable _repositories = new();
    private IUserRepository? _users;
    private IAdminUserRepository? _adminUsers;
    private IAdminRoleRepository? _adminRoles;
    private ITrainRepository? _trains;
    private ITripRepository? _trips;
    private IRailwayPathRepository? _railwayPaths;
    private IStopRepository? _stops;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IUserRepository Users => _users ??= new UserRepository(_context);
    public IAdminUserRepository AdminUsers => _adminUsers ??= new AdminUserRepository(_context);
    public IAdminRoleRepository AdminRoles => _adminRoles ??= new AdminRoleRepository(_context);
    public ITrainRepository Trains => _trains ??= new TrainRepository(_context);
    public ITripRepository Trips => _trips ??= new TripRepository(_context);
    public IRailwayPathRepository RailwayPaths => _railwayPaths ??= new RailwayPathRepository(_context);
    public IStopRepository Stops => _stops ??= new StopRepository(_context);

    public IGenericRepository<T> Repository<T>() where T : BaseEntity
    {
        var type = typeof(T).Name;
        if (!_repositories.ContainsKey(type))
        {
            var repositoryInstance = new GenericRepository<T>(_context);
            _repositories.Add(type, repositoryInstance);
        }
        return (IGenericRepository<T>)_repositories[type]!;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
