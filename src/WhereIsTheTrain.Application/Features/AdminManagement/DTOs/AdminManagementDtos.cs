using System;
using System.Collections.Generic;

namespace WhereIsTheTrain.Application.Features.AdminManagement.DTOs;

public class AdminRoleDetailsDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<AdminRolePrivilegeDto> Privileges { get; set; } = new();
}

public class AdminRolePrivilegeDto
{
    public string Module { get; set; } = string.Empty;
    public bool CanView { get; set; }
    public bool CanAdd { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
}

public class AdminUserDetailsDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool IsSuperAdmin { get; set; }
    public Guid? RoleId { get; set; }
    public string? RoleName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateAdminRoleRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<AdminRolePrivilegeDto> Privileges { get; set; } = new();
}

public class CreateAdminUserRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public Guid? RoleId { get; set; }
}

public class UpdateAdminUserRequestDto
{
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Password { get; set; } // optional
    public Guid? RoleId { get; set; }
}
