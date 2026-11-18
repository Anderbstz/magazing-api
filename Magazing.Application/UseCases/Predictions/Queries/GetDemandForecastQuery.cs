using Magazing.Application.DTOs.Forecasting;
using Magazing.Application.Services;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using MediatR;
using Unit = Magazing.Domain.Entities.Unit;

namespace Magazing.Application.UseCases.Predictions.Queries;

public record GetDemandForecastQuery(Guid ProductId, Guid? WarehouseId) : IRequest<DemandForecastDto>;

internal sealed class GetDemandForecastQueryHandler(
    IUnitOfWork unitOfWork,
    IForecastReadService forecastReadService,
    IDemandForecaster demandForecaster)
    : IRequestHandler<GetDemandForecastQuery, DemandForecastDto>
{
    public async Task<DemandForecastDto> Handle(GetDemandForecastQuery request, CancellationToken cancellationToken)
    {
        var product = await unitOfWork.Repository<Product>().FirstOrDefaultAsync(item => item.ProductId == request.ProductId, cancellationToken)
            ?? throw new KeyNotFoundException("El producto no existe.");

        var unit = await unitOfWork.Repository<Unit>().FirstOrDefaultAsync(item => item.UnitId == product.UnitId, cancellationToken);
        var warehouse = await ResolveWarehouseAsync(request.WarehouseId, cancellationToken);

        var currentWeek = ForecastWindow.CurrentWeekStart();
        var sales = await forecastReadService.GetWeeklySalesAsync(
            ForecastWindow.HistoryStart(currentWeek),
            currentWeek,
            product.ProductId,
            warehouse.WarehouseId,
            cancellationToken);

        var history = ForecastWindow.DenseHistory(sales, product.ProductId, warehouse.WarehouseId, currentWeek);
        var forecast = demandForecaster.Forecast(history.Select(item => item.Quantity).ToList(), currentWeek, ForecastWindow.HorizonWeeks);

        var stock = await unitOfWork.Repository<Stock>().FirstOrDefaultAsync(
            item => item.ProductId == product.ProductId && item.WarehouseId == warehouse.WarehouseId,
            cancellationToken);

        var points = history
            .Select(item => new ForecastPointDto
            {
                PeriodStart = item.WeekStart,
                PeriodEnd = item.WeekStart.AddDays(6),
                ActualQuantity = item.Quantity
            })
            .Concat(forecast.Select(item => new ForecastPointDto
            {
                PeriodStart = item.WeekStart,
                PeriodEnd = item.WeekStart.AddDays(6),
                PredictedQuantity = item.Predicted,
                LowerBound = item.Lower,
                UpperBound = item.Upper
            }))
            .ToList();

        return new DemandForecastDto
        {
            ProductId = product.ProductId,
            ProductName = product.Name,
            UnitCode = unit?.Code ?? string.Empty,
            WarehouseId = warehouse.WarehouseId,
            ModelVersion = demandForecaster.ModelVersion,
            CurrentStock = stock?.Quantity ?? 0,
            Points = points
        };
    }

    private async Task<Warehouse> ResolveWarehouseAsync(Guid? warehouseId, CancellationToken cancellationToken)
    {
        var warehouses = unitOfWork.Repository<Warehouse>();

        if (warehouseId is { } id)
        {
            return await warehouses.FirstOrDefaultAsync(item => item.WarehouseId == id, cancellationToken)
                ?? throw new KeyNotFoundException("El almacen no existe.");
        }

        return (await warehouses.FindAsync(item => item.IsActive, cancellationToken))
            .OrderBy(item => item.Code)
            .FirstOrDefault()
            ?? throw new InvalidOperationException("No hay almacenes activos.");
    }
}
