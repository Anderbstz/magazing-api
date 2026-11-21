using Magazing.Application.DTOs.Reports;
using Magazing.Application.Services;
using Magazing.Application.UseCases.Company;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Sales.Queries;

public record GetSalePdfQuery(Guid SaleId) : IRequest<ExportFile>;

internal sealed class GetSalePdfQueryHandler(
    IUnitOfWork unitOfWork,
    ISalesReadService salesReadService,
    IInvoicePdfService invoicePdfService)
    : IRequestHandler<GetSalePdfQuery, ExportFile>
{
    public async Task<ExportFile> Handle(GetSalePdfQuery request, CancellationToken cancellationToken)
    {
        var sale = await salesReadService.GetSaleAsync(request.SaleId, cancellationToken);
        var company = CompanyReadModel.ToDto(await CompanyReadModel.GetAsync(unitOfWork, cancellationToken));

        return new ExportFile(invoicePdfService.BuildInvoice(company, sale), $"{sale.FullNumber}.pdf");
    }
}
