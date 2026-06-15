namespace WhereIsTheTrain.Infrastructure.Email;

public class EmailSettings
{
    public const string SectionName = "EmailSettings";
    public string SmtpHost { get; set; } = "smtp.gmail.com";
    public int SmtpPort { get; set; } = 587;
    public string SmtpUser { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "Where is the Train";
    public string BaseUrl { get; set; } = "http://localhost:5000";
}
