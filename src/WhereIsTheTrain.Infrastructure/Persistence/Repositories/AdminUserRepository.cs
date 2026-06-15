using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WhereIsTheTrain.Domain.Entities;
using WhereIsTheTrain.Domain.Interfaces;

namespace WhereIsTheTrain.Infrastructure.Persistence.Repositories;

public class AdminUserRepository : GenericRepository<AdminUser>, IAdminUserRepository
{
    public AdminUserRepository(ApplicationDbContext context) : base(context) { }

    public async Task<AdminUser?> GetByEmailWithRoleAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(u => u.Role)
                .ThenInclude(r => r!.Privileges)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<AdminUser?> GetByIdWithRoleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(u => u.Role)
                .ThenInclude(r => r!.Privileges)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<AdminUser>> GetAllWithRoleAsync(CancellationToken cancellationToken = default)
    {
        var list = await _dbSet
            .Include(u => u.Role)
            .ToListAsync(cancellationToken);
        return list;
    }
}
