using Magazing.Application.DTOs.Notifications;
using Magazing.Domain.Entities;
using Magazing.Domain.Notifications;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Emails.Commands;

public record RetryEmailCommand(Guid EmailOutboxId) : IRequest<EmailDto>;

internal sealed class RetryEmailCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<RetryEmailCommand, EmailDto>
{
    public async Task<EmailDto> Handle(RetryEmailCommand request, CancellationToken cancellationToken)
    {
        var email = await unitOfWork.Repository<EmailOutbox>().GetByIdAsync(request.EmailOutboxId, cancellationToken)
            ?? throw new KeyNotFoundException("El correo no existe.");

        if (email.Status != EmailStatuses.Failed)
        {
            throw new InvalidOperationException($"Solo se puede reintentar un correo fallido. Estado actual: {email.Status}.");
        }

        email.Status = EmailStatuses.Pending;
        email.Attempts = 0;
        email.ScheduledAt = DateTime.UtcNow;

        await unitOfWork.CompleteAsync(cancellationToken);

        return EmailReadModel.ToDto(email);
    }
}
