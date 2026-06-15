using MediatR;
using WhereIsTheTrain.Application.Common;
using WhereIsTheTrain.Application.Features.AdminManagement.DTOs;
using WhereIsTheTrain.Domain.Entities;
using WhereIsTheTrain.Domain.Interfaces;
using BCrypt.Net;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WhereIsTheTrain.Application.Features.AdminManagement.Commands;

public class CreateAdminRoleCommandHandler : IRequestHandler<CreateAdminRoleCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateAdminRoleCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<Guid>> Handle(CreateAdminRoleCommand request, CancellationToken cancellationToken)
    {
        var roles = await _unitOfWork.AdminRoles.FindAsync(r => r.Name == request.Name, cancellationToken);
        if (roles.Any())
            return Result<Guid>.Failure("A role with this name already exists.", 409);

        var role = new AdminRole
        {
            Name = request.Name,
            Description = request.Description
        };

        foreach (var p in request.Privileges)
        {
            role.Privileges.Add(new AdminRolePrivilege
            {
                Module = p.Module,
                CanView = p.CanView,
                CanAdd = p.CanAdd,
                CanEdit = p.CanEdit,
                CanDelete = p.CanDelete
            });
        }

        await _unitOfWork.AdminRoles.AddAsync(role, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(role.Id, 201);
    }
}

public class UpdateAdminRoleCommandHandler : IRequestHandler<UpdateAdminRoleCommand, Result<string>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAdminRoleCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<string>> Handle(UpdateAdminRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _unitOfWork.AdminRoles.GetByIdWithPrivilegesAsync(request.Id, cancellationToken);

        if (role == null)
            return Result<string>.Failure("Role not found.", 404);

        if (role.Name != request.Name)
        {
            var otherRoles = await _unitOfWork.AdminRoles.FindAsync(r => r.Name == request.Name, cancellationToken);
            if (otherRoles.Any())
                return Result<string>.Failure("A role with this name already exists.", 409);
        }

        role.Name = request.Name;
        role.Description = request.Description;

        // Clear and reload privileges via custom UnitOfWork repo
        var privilegesRepo = _unitOfWork.Repository<AdminRolePrivilege>();
        foreach (var oldPrivilege in role.Privileges.ToList())
        {
            await privilegesRepo.DeleteAsync(oldPrivilege, cancellationToken);
        }
        role.Privileges.Clear();

        // Add new privileges
        foreach (var p in request.Privileges)
        {
            role.Privileges.Add(new AdminRolePrivilege
            {
                Module = p.Module,
                CanView = p.CanView,
                CanAdd = p.CanAdd,
                CanEdit = p.CanEdit,
                CanDelete = p.CanDelete
            });
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<string>.Success("Role updated successfully.");
    }
}

public class DeleteAdminRoleCommandHandler : IRequestHandler<DeleteAdminRoleCommand, Result<string>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAdminRoleCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<string>> Handle(DeleteAdminRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _unitOfWork.AdminRoles.GetByIdAsync(request.Id, cancellationToken);
        if (role == null)
            return Result<string>.Failure("Role not found.", 404);

        await _unitOfWork.AdminRoles.DeleteAsync(role, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<string>.Success("Role deleted successfully.");
    }
}

public class CreateAdminUserCommandHandler : IRequestHandler<CreateAdminUserCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateAdminUserCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<Guid>> Handle(CreateAdminUserCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.ToLowerInvariant().Trim();
        var existingAdmins = await _unitOfWork.AdminUsers.FindAsync(u => u.Email == email, cancellationToken);
        if (existingAdmins.Any())
            return Result<Guid>.Failure("An admin user with this email already exists.", 409);

        if (request.RoleId.HasValue && !await _unitOfWork.AdminRoles.ExistsAsync(request.RoleId.Value, cancellationToken))
            return Result<Guid>.Failure("Assigned role does not exist.", 400);

        var admin = new AdminUser
        {
            Email = email,
            DisplayName = request.DisplayName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            IsSuperAdmin = false,
            RoleId = request.RoleId
        };

        await _unitOfWork.AdminUsers.AddAsync(admin, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(admin.Id, 201);
    }
}

public class UpdateAdminUserCommandHandler : IRequestHandler<UpdateAdminUserCommand, Result<string>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAdminUserCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<string>> Handle(UpdateAdminUserCommand request, CancellationToken cancellationToken)
    {
        var admin = await _unitOfWork.AdminUsers.GetByIdAsync(request.Id, cancellationToken);
        if (admin == null)
            return Result<string>.Failure("Admin user not found.", 404);

        var email = request.Email.ToLowerInvariant().Trim();
        if (admin.Email != email)
        {
            var existingAdmins = await _unitOfWork.AdminUsers.FindAsync(u => u.Email == email, cancellationToken);
            if (existingAdmins.Any())
                return Result<string>.Failure("An admin user with this email already exists.", 409);
        }

        if (request.RoleId.HasValue && !await _unitOfWork.AdminRoles.ExistsAsync(request.RoleId.Value, cancellationToken))
            return Result<string>.Failure("Assigned role does not exist.", 400);

        admin.Email = email;
        admin.DisplayName = request.DisplayName;
        admin.RoleId = request.RoleId;

        if (!string.IsNullOrEmpty(request.Password))
        {
            admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        }

        await _unitOfWork.AdminUsers.UpdateAsync(admin, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<string>.Success("Admin user updated successfully.");
    }
}

public class DeleteAdminUserCommandHandler : IRequestHandler<DeleteAdminUserCommand, Result<string>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAdminUserCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<string>> Handle(DeleteAdminUserCommand request, CancellationToken cancellationToken)
    {
        var admin = await _unitOfWork.AdminUsers.GetByIdAsync(request.Id, cancellationToken);
        if (admin == null)
            return Result<string>.Failure("Admin user not found.", 404);

        if (admin.IsSuperAdmin)
        {
            // Ensure we don't delete the last Super Admin
            var superAdmins = await _unitOfWork.AdminUsers.FindAsync(u => u.IsSuperAdmin, cancellationToken);
            if (superAdmins.Count <= 1)
            {
                return Result<string>.Failure("Cannot delete the only remaining Super Admin.", 400);
            }
        }

        await _unitOfWork.AdminUsers.DeleteAsync(admin, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<string>.Success("Admin user deleted successfully.");
    }
}
