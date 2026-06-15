using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WhereIsTheTrain.Application.Interfaces;

namespace WhereIsTheTrain.Infrastructure.Email;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendEmailConfirmationAsync(string email, string displayName, string confirmationToken, CancellationToken cancellationToken = default)
    {
        var confirmationLink = $"{_settings.BaseUrl}/api/auth/confirm-email?token={Uri.EscapeDataString(confirmationToken)}";

        var subject = "Confirm your email - Where is the Train";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
                    <h1 style='color: white; margin: 0;'>🚂 Where is the Train</h1>
                </div>
                <div style='padding: 30px; background: #f9f9f9; border-radius: 0 0 10px 10px;'>
                    <h2>Welcome, {displayName}!</h2>
                    <p>Thank you for registering. Please confirm your email address by clicking the button below:</p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{confirmationLink}' style='background: #667eea; color: white; padding: 14px 28px; text-decoration: none; border-radius: 8px; font-size: 16px;'>Confirm Email</a>
                    </div>
                    <p style='color: #666; font-size: 14px;'>If the button doesn't work, copy and paste this link into your browser:<br/>{confirmationLink}</p>
                </div>
            </body>
            </html>";

        try
        {
            using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
            {
                Credentials = new NetworkCredential(_settings.SmtpUser, _settings.SmtpPassword),
                EnableSsl = true
            };

            var message = new MailMessage
            {
                From = new MailAddress(_settings.FromEmail, _settings.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            message.To.Add(email);

            await client.SendMailAsync(message, cancellationToken);
            _logger.LogInformation("Confirmation email sent to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send confirmation email to {Email}", email);
            // Don't throw - email failure shouldn't block registration
            // In production, use a queue/retry mechanism
        }
    }
}
