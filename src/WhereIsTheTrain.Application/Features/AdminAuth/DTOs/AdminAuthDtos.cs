using System;
using System.Collections.Generic;

namespace WhereIsTheTrain.Application.Features.AdminAuth.DTOs;

public class AdminAuthResponseDto
{
    public Guid AdminId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public bool IsSuperAdmin { get; set; }
}

public class AdminProfileDto
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsSuperAdmin { get; set; }
    public string? RoleName { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public List<AdminPrivilegeDto> Privileges { get; set; } = new();
}

public class AdminPrivilegeDto
{
    public string Module { get; set; } = string.Empty;
    public bool CanView { get; set; }
    public bool CanAdd { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
}

public class AdminLoginRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
}
