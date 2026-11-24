using Magazing.Application.DTOs.Catalog;
using Magazing.Application.DTOs.Reports;
using Magazing.Application.Services;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using MediatR;
using Unit = Magazing.Domain.Entities.Unit;

namespace Magazing.Application.UseCases.Products.Queries;

public record GetProductImportTemplateQuery : IRequest<ExportFile>;

internal sealed class GetProductImportTemplateQueryHandler(IUnitOfWork unitOfWork, IExcelReportService excelReportService)
    : IRequestHandler<GetProductImportTemplateQuery, ExportFile>
{
    public async Task<ExportFile> Handle(GetProductImportTemplateQuery request, CancellationToken cancellationToken)
    {
        var categories = (await unitOfWork.Repository<Category>().FindAsync(item => item.IsActive, cancellationToken))
            .Select(item => item.Name).Order().ToList();
        var brands = (await unitOfWork.Repository<Brand>().FindAsync(item => item.IsActive, cancellationToken))
            .Select(item => item.Name).Order().ToList();
        var units = (await unitOfWork.Repository<Unit>().FindAsync(item => true, cancellationToken))
            .OrderBy(item => item.Code).Select(item => (item.Code, item.Name)).ToList();

        var content = excelReportService.BuildProductImportTemplate(new ProductImportCatalogs(categories, brands, units));
        return new ExportFile(content, "plantilla_productos.xlsx");
    }
}
