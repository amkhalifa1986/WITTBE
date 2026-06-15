using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WhereIsTheTrain.Application.Common;
using WhereIsTheTrain.Application.Features.AdminManagement.DTOs;
using WhereIsTheTrain.Domain.Entities;
using WhereIsTheTrain.Domain.Interfaces;

namespace WhereIsTheTrain.Application.Features.AdminManagement.Queries;

public class GetAdminRolesQueryHandler : IRequestHandler<GetAdminRolesQuery, Result<List<AdminRoleDetailsDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAdminRolesQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<List<AdminRoleDetailsDto>>> Handle(GetAdminRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await _unitOfWork.AdminRoles.GetAllWithPrivilegesAsync(cancellationToken);

        var dtos = roles.Select(r => new AdminRoleDetailsDto
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description,
            Privileges = r.Privileges.Select(p => new AdminRolePrivilegeDto
            {
                Module = p.Module,
                CanView = p.CanView,
                CanAdd = p.CanAdd,
                CanEdit = p.CanEdit,
                CanDelete = p.CanDelete
            }).ToList()
        }).OrderBy(r => r.Name).ToList();

        return Result<List<AdminRoleDetailsDto>>.Success(dtos);
    }
}

public class GetAdminUsersQueryHandler : IRequestHandler<GetAdminUsersQuery, Result<List<AdminUserDetailsDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAdminUsersQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<List<AdminUserDetailsDto>>> Handle(GetAdminUsersQuery request, CancellationToken cancellationToken)
    {
        var admins = await _unitOfWork.AdminUsers.GetAllWithRoleAsync(cancellationToken);

        var dtos = admins.Select(u => new AdminUserDetailsDto
        {
            Id = u.Id,
            Email = u.Email,
            DisplayName = u.DisplayName,
            IsSuperAdmin = u.IsSuperAdmin,
            RoleId = u.RoleId,
            RoleName = u.IsSuperAdmin ? "Super Admin" : u.Role?.Name ?? "No Role Assigned",
            CreatedAt = u.CreatedAt
        }).OrderByDescending(u => u.IsSuperAdmin).ThenBy(u => u.DisplayName).ToList();

        return Result<List<AdminUserDetailsDto>>.Success(dtos);
    }
}
