using Magazing.Application.DTOs.Forecasting;
using Magazing.Application.Services;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using Magazing.Domain.Services.Forecasting;
using MediatR;

namespace Magazing.Application.UseCases.Alerts.Commands;

public record AcknowledgeAlertCommand(Guid StockAlertId) : IRequest<StockAlertDto>;

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
