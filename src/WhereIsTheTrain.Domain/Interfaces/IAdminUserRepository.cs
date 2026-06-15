using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WhereIsTheTrain.Domain.Entities;

namespace WhereIsTheTrain.Domain.Interfaces;

public interface IAdminUserRepository : IGenericRepository<AdminUser>
{
    Task<AdminUser?> GetByEmailWithRoleAsync(string email, CancellationToken cancellationToken = default);
    Task<AdminUser?> GetByIdWithRoleAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AdminUser>> GetAllWithRoleAsync(CancellationToken cancellationToken = default);
}
