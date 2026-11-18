using Magazing.Application.Services;
using Magazing.Domain.Entities;
using Magazing.Domain.Notifications;
using Magazing.Infrastructure.Context;
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

        foreach (var email in batch)
        {
            await SendAsync(sender, email, settings.MaxAttempts, cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return batch.Count;
    }

    private async Task SendAsync(IEmailSender sender, EmailOutbox email, int maxAttempts, CancellationToken cancellationToken)
    {
        email.Attempts += 1;

        try
        {
            email.ProviderMessageId = await sender.SendAsync(new EmailMessage(email.ToEmail, email.Subject, email.HtmlBody), cancellationToken);
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
}
