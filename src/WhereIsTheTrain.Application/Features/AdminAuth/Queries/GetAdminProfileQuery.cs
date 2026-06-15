using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WhereIsTheTrain.Application.Common;
using WhereIsTheTrain.Application.Features.AdminAuth.DTOs;
using WhereIsTheTrain.Domain.Entities;
using WhereIsTheTrain.Domain.Interfaces;

namespace WhereIsTheTrain.Application.Features.AdminAuth.Queries;

public record GetAdminProfileQuery(Guid AdminId) : IRequest<Result<AdminProfileDto>>;

public class GetAdminProfileQueryHandler : IRequestHandler<GetAdminProfileQuery, Result<AdminProfileDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAdminProfileQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AdminProfileDto>> Handle(GetAdminProfileQuery request, CancellationToken cancellationToken)
    {
        var admin = await _unitOfWork.AdminUsers.GetByIdWithRoleAsync(request.AdminId, cancellationToken);

        if (admin == null)
            return Result<AdminProfileDto>.Failure("Admin not found.", 404);

        var dto = new AdminProfileDto
        {
            Id = admin.Id,
            DisplayName = admin.DisplayName,
            Email = admin.Email,
            IsSuperAdmin = admin.IsSuperAdmin,
            RoleName = admin.Role?.Name,
            AvatarUrl = admin.AvatarUrl,
            Bio = admin.Bio,
            Privileges = admin.IsSuperAdmin 
                ? GetSuperAdminPrivileges() 
                : admin.Role?.Privileges.Select(p => new AdminPrivilegeDto
                {
                    Module = p.Module,
                    CanView = p.CanView,
                    CanAdd = p.CanAdd,
                    CanEdit = p.CanEdit,
                    CanDelete = p.CanDelete
                }).ToList() ?? new List<AdminPrivilegeDto>()
        };

        return Result<AdminProfileDto>.Success(dto);
    }

    private List<AdminPrivilegeDto> GetSuperAdminPrivileges()
    {
        var modules = new[] { "Dashboard", "Users", "Trains", "Trips", "Stops", "Lookups", "LostFound", "Suggestions", "Disruptions", "RailwayPaths", "Updates", "Settings", "AdminManagement" };
        return modules.Select(m => new AdminPrivilegeDto
        {
            Module = m,
            CanView = true,
            CanAdd = true,
            CanEdit = true,
            CanDelete = true
        }).ToList();
    }
}
