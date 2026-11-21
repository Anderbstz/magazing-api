using Magazing.Application.Services;
using Microsoft.Extensions.Logging;

namespace Magazing.Infrastructure.Adapters.Email;

public class LogEmailSender(ILogger<LogEmailSender> logger) : IEmailSender
{
    public Task<string> SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        var attachments = message.Attachments is { Count: > 0 } list
            ? string.Join(", ", list.Select(item => $"{item.FileName} ({item.Content.Length} bytes)"))
            : "sin adjuntos";

        logger.LogInformation("Correo simulado para {To}: {Subject} [{Attachments}]", message.To, message.Subject, attachments);
        return Task.FromResult($"log-{Guid.NewGuid():N}");
    }
}
