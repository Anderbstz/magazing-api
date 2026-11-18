using Magazing.Application.Services;
using Microsoft.Extensions.Logging;

namespace Magazing.Infrastructure.Adapters.Email;

public class LogEmailSender(ILogger<LogEmailSender> logger) : IEmailSender
{
    public Task<string> SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Correo simulado para {To}: {Subject}", message.To, message.Subject);
        return Task.FromResult($"log-{Guid.NewGuid():N}");
    }
}
