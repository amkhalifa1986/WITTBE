using System;
using WhereIsTheTrain.Domain.Common;

namespace WhereIsTheTrain.Domain.Entities;

public class SystemLog : BaseEntity
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string LogLevel { get; set; } = "Info"; // Error, Warning, Info
    public string Source { get; set; } = string.Empty; // API, Frontend, Mobile, AdminAction
    public string Target { get; set; } = string.Empty; // API Endpoint or Page/Screen Name
    public Guid? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public string? StackTrace { get; set; }
    public bool IsArchived { get; set; } = false;
}
