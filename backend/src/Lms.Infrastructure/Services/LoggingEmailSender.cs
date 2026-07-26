using Lms.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace Lms.Infrastructure.Services;

/// <summary>
/// Development email sender that logs messages instead of sending them. Swap for a real
/// SMTP/provider implementation in production (registration point in DependencyInjection).
/// </summary>
public class LoggingEmailSender : IEmailSender
{
    private readonly ILogger<LoggingEmailSender> _logger;

    public LoggingEmailSender(ILogger<LoggingEmailSender> logger) => _logger = logger;

    public Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[EMAIL] To: {To} | Subject: {Subject}\n{Body}", to, subject, htmlBody);
        return Task.CompletedTask;
    }
}
