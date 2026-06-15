using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WhereIsTheTrain.Domain.Entities;
using WhereIsTheTrain.Domain.Interfaces;

namespace WhereIsTheTrain.Infrastructure.Persistence.Repositories;

public class AdminRoleRepository : GenericRepository<AdminRole>, IAdminRoleRepository
{
    public AdminRoleRepository(ApplicationDbContext context) : base(context) { }

    public async Task<AdminRole?> GetByIdWithPrivilegesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.Privileges)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<AdminRole>> GetAllWithPrivilegesAsync(CancellationToken cancellationToken = default)
    {
        var list = await _dbSet
            .Include(r => r.Privileges)
            .ToListAsync(cancellationToken);
        return list;
    }
}
