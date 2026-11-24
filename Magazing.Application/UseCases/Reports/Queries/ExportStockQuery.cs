using Magazing.Application.Services.Common;
using Magazing.Application.DTOs.Inventory;
using Magazing.Application.DTOs.Reports;
using Magazing.Application.Services.Inventory;
using Magazing.Application.Services;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Reports.Queries;

public record ExportStockQuery(Guid WarehouseId, string? Status = null) : IRequest<ExportFile>;

internal sealed class ExportStockQueryHandler(
    IUnitOfWork unitOfWork,
    IInventoryReadService inventoryReadService,
    IExcelReportService excelReportService)
    : IRequestHandler<ExportStockQuery, ExportFile>
{
    public async Task<ExportFile> Handle(ExportStockQuery request, CancellationToken cancellationToken)
    {
        var status = string.IsNullOrWhiteSpace(request.Status) ? null : request.Status.Trim().ToUpper();
        if (status is not null && !StockStatus.IsValid(status))
        {
            throw new InvalidOperationException("El estado debe ser OK, LOW u OUT.");
        }

        var warehouse = await unitOfWork.Repository<Warehouse>().FirstOrDefaultAsync(item => item.WarehouseId == request.WarehouseId, cancellationToken)
            ?? throw new KeyNotFoundException("El almacén no existe.");

        var items = new List<StockItemDto>();
        for (var page = 1; ; page++)
        {
            var result = await inventoryReadService.GetStocksAsync(warehouse.WarehouseId, status, null, page, ExportNames.PageSize, cancellationToken);
            items.AddRange(result.Items);

            if (page >= result.TotalPages || items.Count >= ExportNames.MaxRows)
            {
                break;
            }
        }

        var summary = await inventoryReadService.GetStockSummaryAsync(warehouse.WarehouseId, cancellationToken);
        var subtitle = $"{warehouse.Code} · {warehouse.Name} · al {BusinessTime.ToLima(DateTime.UtcNow):dd/MM/yyyy HH:mm}";
        var content = excelReportService.BuildStockReport(subtitle, summary, items);

        return new ExportFile(content, $"stock_{warehouse.Code}_{ExportNames.Stamp()}.xlsx");
    }
}
