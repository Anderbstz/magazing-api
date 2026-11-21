using Magazing.Application.DTOs.Inventory;
using Magazing.Application.DTOs.Reports;
using Magazing.Application.Services;
using Magazing.Application.UseCases.Inventory;
using MediatR;

namespace Magazing.Application.UseCases.Reports.Queries;

public record ExportKardexQuery(
    Guid? ProductId = null,
    Guid? WarehouseId = null,
    string? Type = null,
    DateOnly? From = null,
    DateOnly? To = null) : IRequest<ExportFile>;

internal sealed class ExportKardexQueryHandler(IInventoryReadService inventoryReadService, IExcelReportService excelReportService)
    : IRequestHandler<ExportKardexQuery, ExportFile>
{
    public async Task<ExportFile> Handle(ExportKardexQuery request, CancellationToken cancellationToken)
    {
        var movements = new List<MovementDto>();

        for (var page = 1; ; page++)
        {
            var filter = MovementFilterFactory.Create(
                request.ProductId, request.WarehouseId, request.Type, request.From, request.To, page, ExportNames.PageSize);
            var result = await inventoryReadService.GetMovementsAsync(filter, cancellationToken);
            movements.AddRange(result.Items);

            if (page >= result.TotalPages)
            {
                break;
            }

            if (movements.Count >= ExportNames.MaxRows)
            {
                throw new InvalidOperationException($"El kardex supera {ExportNames.MaxRows} movimientos. Acote el rango de fechas.");
            }
        }

        var range = (request.From, request.To) switch
        {
            ({ } from, { } to) => $"Del {from:dd/MM/yyyy} al {to:dd/MM/yyyy}",
            ({ } from, null) => $"Desde el {from:dd/MM/yyyy}",
            (null, { } to) => $"Hasta el {to:dd/MM/yyyy}",
            _ => "Todos los movimientos"
        };

        var content = excelReportService.BuildKardexReport($"{range} · {movements.Count} movimientos", movements);

        return new ExportFile(content, $"kardex_{ExportNames.Stamp()}.xlsx");
    }
}
