using Magazing.Domain.Entities;
using Magazing.Domain.Notifications;
using Magazing.Domain.Repositories;

namespace Magazing.Application.Notifications;

internal static class EmailQueue
{
    public static async Task EnqueueAsync(
        IUnitOfWork unitOfWork,
        string? to,
        EmailContent content,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(to))
        {
            return;
        }

        var now = DateTime.UtcNow;

        await unitOfWork.Repository<EmailOutbox>().AddAsync(new EmailOutbox
        {
            EmailOutboxId = Guid.NewGuid(),
            ToEmail = to.Trim().ToLower(),
            Subject = content.Subject,
            HtmlBody = content.HtmlBody,
            Template = content.Template,
            Status = EmailStatuses.Pending,
            Attempts = 0,
            ScheduledAt = now,
            CreatedAt = now
        }, cancellationToken);
    }
}

internal sealed record EmailContent(string Template, string Subject, string HtmlBody);
