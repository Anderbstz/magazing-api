using Magazing.Application.Common;
using Magazing.Application.DTOs.Sales;
using Magazing.Application.Services;
using Magazing.Domain.Common;
using Magazing.Domain.Sales;
using MediatR;

namespace Magazing.Application.UseCases.Sales.Queries;

public record GetSalesQuery(
    string? Status = null,
    string? DocumentType = null,
    string? Search = null,
    DateOnly? From = null,
    DateOnly? To = null,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<SaleSummaryDto>>;

internal sealed class GetSalesQueryHandler(ISalesReadService salesReadService)
    : IRequestHandler<GetSalesQuery, PagedResult<SaleSummaryDto>>
{
    public Task<PagedResult<SaleSummaryDto>> Handle(GetSalesQuery request, CancellationToken cancellationToken)
    {
        var status = string.IsNullOrWhiteSpace(request.Status) ? null : request.Status.Trim().ToUpper();
        if (status is not null && !SaleStatuses.IsValid(status))
        {
            throw new InvalidOperationException("El estado debe ser COMPLETED o CANCELLED.");
        }

        var documentType = string.IsNullOrWhiteSpace(request.DocumentType) ? null : request.DocumentType.Trim().ToUpper();
        if (documentType is not null && !InvoiceTypes.IsValid(documentType))
        {
            throw new InvalidOperationException("El comprobante debe ser BOLETA o FACTURA.");
        }

        if (request.From is { } from && request.To is { } to && from > to)
        {
            throw new InvalidOperationException("La fecha inicial no puede ser mayor que la fecha final.");
        }

        return salesReadService.GetSalesAsync(new SaleFilter(
            status,
            documentType,
            request.Search,
            request.From is { } fromDate ? BusinessTime.StartOfDayUtc(fromDate) : null,
            request.To is { } toDate ? BusinessTime.StartOfNextDayUtc(toDate) : null,
            request.Page,
            request.PageSize), cancellationToken);
    }
}
