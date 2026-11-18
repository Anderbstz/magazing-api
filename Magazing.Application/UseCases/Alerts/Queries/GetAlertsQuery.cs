using Magazing.Application.DTOs.Forecasting;
using Magazing.Application.Services;
using Magazing.Domain.Forecasting;
using MediatR;

namespace Magazing.Application.UseCases.Alerts.Queries;

public record GetAlertsQuery(string? Status = null) : IRequest<List<StockAlertDto>>;

internal sealed class GetAlertsQueryHandler(IForecastReadService forecastReadService)
    : IRequestHandler<GetAlertsQuery, List<StockAlertDto>>
{
    public Task<List<StockAlertDto>> Handle(GetAlertsQuery request, CancellationToken cancellationToken)
    {
        var status = string.IsNullOrWhiteSpace(request.Status) ? null : request.Status.Trim().ToUpper();
        if (status is not null && !AlertStatuses.IsValid(status))
        {
            throw new InvalidOperationException("El estado debe ser OPEN, ACKNOWLEDGED o RESOLVED.");
        }

        return forecastReadService.GetAlertsAsync(status, null, cancellationToken);
    }
}
