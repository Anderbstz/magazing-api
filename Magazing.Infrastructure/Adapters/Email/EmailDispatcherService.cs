using Magazing.Application.Services;
using Magazing.Application.UseCases.Sales.Queries;
using Magazing.Domain.Entities;
using Magazing.Domain.Notifications;
using Magazing.Infrastructure.Context;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Magazing.Infrastructure.Adapters.Email;

public class EmailDispatcherService(
    IServiceScopeFactory scopeFactory,
    IOptions<EmailOptions> options,
    ILogger<EmailDispatcherService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.DispatcherEnabled)
        {
            logger.LogInformation("El despachador de correos esta deshabilitado.");
            return;
        }

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(Math.Max(options.Value.DispatchIntervalSeconds, 5)));

        do
        {
            try
            {
                await DispatchBatchAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Error al despachar la cola de correos.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    public async Task<int> DispatchBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MagazingDbContext>();
        var sender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
        var settings = options.Value;

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        var batch = await context.EmailOutboxes
            .FromSqlInterpolated($"""
                SELECT * FROM email_outbox
                WHERE status = {EmailStatuses.Pending} AND scheduled_at <= now()
                ORDER BY scheduled_at
                LIMIT {settings.BatchSize}
                FOR UPDATE SKIP LOCKED
                """)
            .ToListAsync(cancellationToken);

        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();

        foreach (var email in batch)
        {
            await SendAsync(sender, mediator, email, settings.MaxAttempts, cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return batch.Count;
    }

    private async Task SendAsync(IEmailSender sender, ISender mediator, EmailOutbox email, int maxAttempts, CancellationToken cancellationToken)
    {
        email.Attempts += 1;

        try
        {
            var attachments = await BuildAttachmentsAsync(mediator, email, cancellationToken);
            email.ProviderMessageId = await sender.SendAsync(
                new EmailMessage(email.ToEmail, email.Subject, email.HtmlBody, attachments),
                cancellationToken);
            email.Status = EmailStatuses.Sent;
            email.SentAt = DateTime.UtcNow;
            email.LastError = null;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            email.LastError = ex.Message.Length > 1000 ? ex.Message[..1000] : ex.Message;

            if (email.Attempts >= maxAttempts)
            {
                email.Status = EmailStatuses.Failed;
                logger.LogWarning("El correo {EmailId} fallo definitivamente tras {Attempts} intentos.", email.EmailOutboxId, email.Attempts);
            }
            else
            {
                email.ScheduledAt = DateTime.UtcNow.AddMinutes(Math.Pow(2, email.Attempts));
            }
        }
    }

    private static async Task<IReadOnlyList<EmailAttachment>?> BuildAttachmentsAsync(ISender mediator, EmailOutbox email, CancellationToken cancellationToken)
    {
        if (email.ReferenceType != EmailReferenceTypes.Sale || email.ReferenceId is not { } saleId)
        {
            return null;
        }

        var pdf = await mediator.Send(new GetSalePdfQuery(saleId), cancellationToken);
        return [new EmailAttachment(pdf.FileName, pdf.Content)];
    }
}
