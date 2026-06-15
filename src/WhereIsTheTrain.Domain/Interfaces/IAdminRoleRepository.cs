using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WhereIsTheTrain.Domain.Entities;

namespace WhereIsTheTrain.Domain.Interfaces;

public interface IAdminRoleRepository : IGenericRepository<AdminRole>
{
    Task<AdminRole?> GetByIdWithPrivilegesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AdminRole>> GetAllWithPrivilegesAsync(CancellationToken cancellationToken = default);
}
