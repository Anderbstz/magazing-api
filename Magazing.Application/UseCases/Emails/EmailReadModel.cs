using Magazing.Application.DTOs.Notifications;
using Magazing.Domain.Entities;

namespace Magazing.Application.UseCases.Emails;

internal static class EmailReadModel
{
    public static EmailDto ToDto(EmailOutbox email)
    {
        return new EmailDto
        {
            EmailOutboxId = email.EmailOutboxId,
            ToEmail = email.ToEmail,
            Subject = email.Subject,
            HtmlBody = email.HtmlBody,
            Template = email.Template,
            ReferenceType = email.ReferenceType,
            ReferenceId = email.ReferenceId,
            Status = email.Status,
            Attempts = email.Attempts,
            LastError = email.LastError,
            ProviderMessageId = email.ProviderMessageId,
            ScheduledAt = email.ScheduledAt,
            SentAt = email.SentAt,
            CreatedAt = email.CreatedAt
        };
    }
}
