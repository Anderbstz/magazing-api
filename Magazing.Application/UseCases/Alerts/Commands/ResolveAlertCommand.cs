using Magazing.Application.DTOs.Forecasting;
using Magazing.Application.Services;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using Magazing.Domain.Services.Forecasting;
using MediatR;

namespace Magazing.Application.UseCases.Alerts.Commands;

public record ResolveAlertCommand(Guid StockAlertId, Guid UserId) : IRequest<StockAlertDto>;

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
