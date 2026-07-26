namespace Lms.Application.Abstractions;

/// <summary>Sends transactional emails (verification, password reset, ...).</summary>
public interface IEmailSender
{
    Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default);
}
