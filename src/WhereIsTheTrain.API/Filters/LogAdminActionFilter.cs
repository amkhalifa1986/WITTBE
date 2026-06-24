using System;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using WhereIsTheTrain.Domain.Entities;
using WhereIsTheTrain.Infrastructure.Persistence;

namespace WhereIsTheTrain.API.Filters;

public class LogAdminActionFilter : IAsyncActionFilter
{
    private readonly ApplicationDbContext _context;

    public LogAdminActionFilter(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var resultContext = await next();

        var request = context.HttpContext.Request;
        var method = request.Method.ToUpperInvariant();

        if (method == "POST" || method == "PUT" || method == "DELETE" || method == "PATCH")
        {
            var isSuccess = resultContext.Exception == null && 
                            resultContext.HttpContext.Response.StatusCode >= 200 && 
                            resultContext.HttpContext.Response.StatusCode < 400;

            var controllerName = context.Controller.GetType().Name;
            var isApiAdmin = request.Path.Value?.StartsWith("/api/admin", StringComparison.OrdinalIgnoreCase) == true || 
                             controllerName.StartsWith("Admin", StringComparison.OrdinalIgnoreCase);

            var isSystemLogsController = controllerName.StartsWith("SystemLogs", StringComparison.OrdinalIgnoreCase);

            if (isApiAdmin && !isSystemLogsController && isSuccess)
            {
                Guid? userId = null;
                string? userEmail = null;

                var user = context.HttpContext.User;
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim != null && Guid.TryParse(userIdClaim, out var parsedId))
                {
                    userId = parsedId;
                }

                userEmail = user.FindFirst(ClaimTypes.Email)?.Value;

                var log = new SystemLog
                {
                    Timestamp = DateTime.UtcNow,
                    LogLevel = "Info",
                    Source = "AdminAction",
                    Target = request.Path.Value ?? string.Empty,
                    UserId = userId,
                    UserEmail = userEmail,
                    Description = $"Admin executed {method} on {request.Path.Value}. Action: {context.ActionDescriptor.DisplayName}."
                };

                try
                {
                    var serializedArgs = JsonSerializer.Serialize(context.ActionArguments);
                    if (serializedArgs.Length > 1000)
                    {
                        serializedArgs = serializedArgs.Substring(0, 1000) + "...";
                    }
                    log.Description += $" Payload: {serializedArgs}";
                }
                catch { }

                _context.Set<SystemLog>().Add(log);
                await _context.SaveChangesAsync();
            }
        }
    }
}
