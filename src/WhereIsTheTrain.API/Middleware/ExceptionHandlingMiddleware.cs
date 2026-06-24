using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text.Json;
using System.Security.Claims;

namespace WhereIsTheTrain.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation error: {Errors}", ex.Message);
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/json";

            var errors = ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
            var response = new { IsSuccess = false, Error = "Validation failed", Details = errors };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized: {Message}", ex.Message);
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            context.Response.ContentType = "application/json";

            var response = new { IsSuccess = false, Error = ex.Message };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");

            try
            {
                var dbContext = context.RequestServices.GetRequiredService<WhereIsTheTrain.Infrastructure.Persistence.ApplicationDbContext>();
                
                Guid? userId = null;
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim != null && Guid.TryParse(userIdClaim, out var parsedId))
                {
                    userId = parsedId;
                }

                var userEmail = context.User.FindFirst(ClaimTypes.Email)?.Value;

                var log = new WhereIsTheTrain.Domain.Entities.SystemLog
                {
                    Timestamp = DateTime.UtcNow,
                    LogLevel = "Error",
                    Source = "API",
                    Target = context.Request.Path.Value ?? string.Empty,
                    UserId = userId,
                    UserEmail = userEmail,
                    Description = $"Unhandled Exception: {ex.Message}",
                    ErrorMessage = ex.Message,
                    StackTrace = ex.StackTrace
                };

                dbContext.Set<WhereIsTheTrain.Domain.Entities.SystemLog>().Add(log);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception dbEx)
            {
                _logger.LogError(dbEx, "Failed to log unhandled exception to database");
            }

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var response = new { IsSuccess = false, Error = "An unexpected error occurred." };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
