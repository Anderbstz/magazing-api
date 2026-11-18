using Magazing.Application.DTOs.Forecasting;
using Magazing.Application.Services;
using Magazing.Domain.Entities;
using Magazing.Domain.Repositories;
using MediatR;

namespace Magazing.Application.UseCases.Predictions.Commands;

public record RunPredictionsCommand : IRequest<RunPredictionsResultDto>;

internal sealed class RunPredictionsCommandHandler(
    IUnitOfWork unitOfWork,
    IForecastReadService forecastReadService,
    IDemandForecaster demandForecaster)
    : IRequestHandler<RunPredictionsCommand, RunPredictionsResultDto>
{
    public async Task<RunPredictionsResultDto> Handle(RunPredictionsCommand request, CancellationToken cancellationToken)
    {
        var currentWeek = ForecastWindow.CurrentWeekStart();
        var historyStart = ForecastWindow.HistoryStart(currentWeek);
        var horizonEnd = currentWeek.AddDays(7 * ForecastWindow.HorizonWeeks);
        var modelVersion = demandForecaster.ModelVersion;

        var products = (await unitOfWork.Repository<Product>().FindAsync(item => item.IsActive, cancellationToken)).ToList();
        var warehouses = (await unitOfWork.Repository<Warehouse>().FindAsync(item => item.IsActive, cancellationToken)).ToList();
        var sales = await forecastReadService.GetWeeklySalesAsync(historyStart, currentWeek, null, null, cancellationToken);

        var predictions = unitOfWork.Repository<DemandPrediction>();
        var existing = (await predictions.FindAsync(
                item => item.ModelVersion == modelVersion && item.PeriodStart >= historyStart && item.PeriodStart < horizonEnd,
                cancellationToken))
            .ToDictionary(item => (item.ProductId, item.WarehouseId, item.PeriodStart));

        var now = DateTime.UtcNow;
        var written = 0;
        var actualsUpdated = 0;

        foreach (var product in products)
        {
            foreach (var warehouse in warehouses)
            {
                var history = ForecastWindow.DenseHistory(sales, product.ProductId, warehouse.WarehouseId, currentWeek);

                foreach (var (weekStart, quantity) in history)
                {
                    if (existing.TryGetValue((product.ProductId, warehouse.WarehouseId, weekStart), out var past) && past.ActualQuantity is null)
                    {
                        past.ActualQuantity = quantity;
                        predictions.Update(past);
                        actualsUpdated++;
                    }
                }

                var forecast = demandForecaster.Forecast(history.Select(item => item.Quantity).ToList(), currentWeek, ForecastWindow.HorizonWeeks);

                foreach (var week in forecast)
                {
                    if (existing.TryGetValue((product.ProductId, warehouse.WarehouseId, week.WeekStart), out var prediction))
                    {
                        prediction.PredictedQuantity = week.Predicted;
                        prediction.LowerBound = week.Lower;
                        prediction.UpperBound = week.Upper;
                        predictions.Update(prediction);
                    }
                    else
                    {
                        await predictions.AddAsync(new DemandPrediction
                        {
                            DemandPredictionId = Guid.NewGuid(),
                            ProductId = product.ProductId,
                            WarehouseId = warehouse.WarehouseId,
                            ModelVersion = modelVersion,
                            PeriodStart = week.WeekStart,
                            PeriodEnd = week.WeekStart.AddDays(6),
                            PredictedQuantity = week.Predicted,
                            LowerBound = week.Lower,
                            UpperBound = week.Upper,
                            CreatedAt = now
                        }, cancellationToken);
                    }

                    written++;
                }
            }
        }

        await unitOfWork.CompleteAsync(cancellationToken);

        return new RunPredictionsResultDto
        {
            Created = written,
            ModelVersion = modelVersion,
            ActualsUpdated = actualsUpdated
        };
    }
}
