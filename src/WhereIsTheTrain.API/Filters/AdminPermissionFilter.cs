using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using WhereIsTheTrain.Domain.Entities;
using WhereIsTheTrain.Infrastructure.Persistence;

namespace WhereIsTheTrain.API.Filters;

public class AdminPermissionFilter : IAsyncAuthorizationFilter
{
    private readonly ApplicationDbContext _context;
    private readonly string _module;
    private readonly string _action;

    public AdminPermissionFilter(ApplicationDbContext context, string module, string action)
    {
        _context = context;
        _module = module;
        _action = action;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        // Must have Role = Admin
        var roleClaim = user.FindFirst(ClaimTypes.Role)?.Value;
        if (roleClaim != "Admin")
        {
            context.Result = new ForbidResult();
            return;
        }

        // Get admin ID
        var adminIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(adminIdClaim) || !Guid.TryParse(adminIdClaim, out var adminId))
        {
            context.Result = new ForbidResult();
            return;
        }

        // Fetch admin details including privileges
        var admin = await _context.Set<AdminUser>()
            .Include(a => a.Role)
                .ThenInclude(r => r!.Privileges)
            .FirstOrDefaultAsync(a => a.Id == adminId);

        if (admin == null)
        {
            context.Result = new ForbidResult();
            return;
        }

        // Super Admin bypasses all checks
        if (admin.IsSuperAdmin)
        {
            return;
        }

        // If regular admin but doesn't have a role, reject
        if (admin.Role == null)
        {
            context.Result = new ForbidResult();
            return;
        }

        // Check module privileges
        var privilege = admin.Role.Privileges.FirstOrDefault(p => p.Module.Equals(_module, StringComparison.OrdinalIgnoreCase));
        if (privilege == null)
        {
            context.Result = new ForbidResult();
            return;
        }

        bool hasPermission = false;
        if (_action.Equals("View", StringComparison.OrdinalIgnoreCase)) hasPermission = privilege.CanView;
        else if (_action.Equals("Add", StringComparison.OrdinalIgnoreCase)) hasPermission = privilege.CanAdd;
        else if (_action.Equals("Edit", StringComparison.OrdinalIgnoreCase)) hasPermission = privilege.CanEdit;
        else if (_action.Equals("Delete", StringComparison.OrdinalIgnoreCase)) hasPermission = privilege.CanDelete;

        if (!hasPermission)
        {
            context.Result = new ForbidResult();
        }
    }
}
