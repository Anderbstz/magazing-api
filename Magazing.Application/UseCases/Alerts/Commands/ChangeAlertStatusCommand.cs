using Magazing.Application.DTOs.Forecasting;
using Magazing.Application.Services;
using Magazing.Domain.Entities;
using Magazing.Domain.Forecasting;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Alerts.Commands;

public record AcknowledgeAlertCommand(Guid StockAlertId) : IRequest<StockAlertDto>;

public record ResolveAlertCommand(Guid StockAlertId, Guid UserId) : IRequest<StockAlertDto>;

internal sealed class AcknowledgeAlertCommandHandler(IUnitOfWork unitOfWork, IForecastReadService forecastReadService)
    : IRequestHandler<AcknowledgeAlertCommand, StockAlertDto>
{
    public async Task<StockAlertDto> Handle(AcknowledgeAlertCommand request, CancellationToken cancellationToken)
    {
        var alert = await unitOfWork.Repository<StockAlert>().GetByIdAsync(request.StockAlertId, cancellationToken)
            ?? throw new KeyNotFoundException("La alerta no existe.");

        if (alert.Status != AlertStatuses.Open)
        {
            throw new InvalidOperationException($"Solo se puede marcar como vista una alerta abierta. Estado actual: {alert.Status}.");
        }

        alert.Status = AlertStatuses.Acknowledged;
        await unitOfWork.CompleteAsync(cancellationToken);

        return (await forecastReadService.GetAlertsAsync(null, [alert.StockAlertId], cancellationToken))[0];
    }
}

internal sealed class ResolveAlertCommandHandler(IUnitOfWork unitOfWork, IForecastReadService forecastReadService)
    : IRequestHandler<ResolveAlertCommand, StockAlertDto>
{
    public async Task<StockAlertDto> Handle(ResolveAlertCommand request, CancellationToken cancellationToken)
    {
        var alert = await unitOfWork.Repository<StockAlert>().GetByIdAsync(request.StockAlertId, cancellationToken)
            ?? throw new KeyNotFoundException("La alerta no existe.");

        if (alert.Status == AlertStatuses.Resolved)
        {
            throw new InvalidOperationException("La alerta ya está resuelta.");
        }

        alert.Status = AlertStatuses.Resolved;
        alert.ResolvedBy = request.UserId;
        alert.ResolvedAt = DateTime.UtcNow;
        await unitOfWork.CompleteAsync(cancellationToken);

        return (await forecastReadService.GetAlertsAsync(null, [alert.StockAlertId], cancellationToken))[0];
    }
}
