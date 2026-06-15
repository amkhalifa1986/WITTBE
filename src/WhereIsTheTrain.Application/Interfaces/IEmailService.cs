namespace WhereIsTheTrain.Application.Interfaces;

public interface IEmailService
{
    Task SendEmailConfirmationAsync(string email, string displayName, string confirmationToken, CancellationToken cancellationToken = default);
}
