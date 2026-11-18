namespace Magazing.Application.Services;

public interface IEmailSender
{
    Task<string> SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}

public record EmailMessage(string To, string Subject, string HtmlBody);
