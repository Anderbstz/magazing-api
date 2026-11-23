using Magazing.Application.DTOs.Forecasting;
using Magazing.Application.Services.Notifications;
using Magazing.Application.Services.Security;
using Magazing.Application.Services;
using Magazing.Domain.Entities;
using Magazing.Domain.Services.Forecasting;
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

        var stocks = (await unitOfWork.Repository<Stock>().GetAllAsync(cancellationToken))
            .ToDictionary(item => (item.ProductId, item.WarehouseId), item => item.Quantity);

        var activeAlerts = (await unitOfWork.Repository<StockAlert>().FindAsync(
                item => item.Status == AlertStatuses.Open || item.Status == AlertStatuses.Acknowledged,
                cancellationToken))
            .Select(item => (item.ProductId, item.WarehouseId, item.AlertType))
            .ToHashSet();

        var predictions = unitOfWork.Repository<DemandPrediction>();
        var existing = (await predictions.FindAsync(
                item => item.ModelVersion == modelVersion && item.PeriodStart >= historyStart && item.PeriodStart < horizonEnd,
                cancellationToken))
            .ToDictionary(item => (item.ProductId, item.WarehouseId, item.PeriodStart));

        var now = DateTime.UtcNow;
        var written = 0;
        var actualsUpdated = 0;
        var newAlerts = new List<(StockAlert Alert, Product Product, Warehouse Warehouse)>();

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
                DemandPrediction? currentWeekPrediction = null;

                foreach (var week in forecast)
                {
                    if (!existing.TryGetValue((product.ProductId, warehouse.WarehouseId, week.WeekStart), out var prediction))
                    {
                        prediction = new DemandPrediction
                        {
                            DemandPredictionId = Guid.NewGuid(),
                            ProductId = product.ProductId,
                            WarehouseId = warehouse.WarehouseId,
                            ModelVersion = modelVersion,
                            PeriodStart = week.WeekStart,
                            PeriodEnd = week.WeekStart.AddDays(6),
                            CreatedAt = now
                        };
                        await predictions.AddAsync(prediction, cancellationToken);
                    }
                    else
                    {
                        predictions.Update(prediction);
                    }

                    prediction.PredictedQuantity = week.Predicted;
                    prediction.LowerBound = week.Lower;
                    prediction.UpperBound = week.Upper;
                    currentWeekPrediction ??= prediction;
                    written++;
                }

                var stock = stocks.GetValueOrDefault((product.ProductId, warehouse.WarehouseId));
                var nextWeekDemand = forecast[0].Predicted;
                var horizonDemand = forecast.Sum(item => item.Predicted);

                var shortageQuantity = StockAlertRules.PredictedShortageOrderQuantity(stock, nextWeekDemand, horizonDemand, product.MinStock);
                if (shortageQuantity is not null && activeAlerts.Add((product.ProductId, warehouse.WarehouseId, AlertTypes.PredictedShortage)))
                {
                    newAlerts.Add((NewAlert(product, warehouse, AlertTypes.PredictedShortage, stock, nextWeekDemand, shortageQuantity, currentWeekPrediction, now), product, warehouse));
                }

                var lowStockQuantity = StockAlertRules.LowStockOrderQuantity(stock, product.MinStock, product.MaxStock);
                if (lowStockQuantity is not null && activeAlerts.Add((product.ProductId, warehouse.WarehouseId, AlertTypes.LowStock)))
                {
                    newAlerts.Add((NewAlert(product, warehouse, AlertTypes.LowStock, stock, product.MinStock, lowStockQuantity, null, now), product, warehouse));
                }
            }
        }

        foreach (var (alert, _, _) in newAlerts)
        {
            await unitOfWork.Repository<StockAlert>().AddAsync(alert, cancellationToken);
        }

        await NotifyAsync(newAlerts, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);

        return new RunPredictionsResultDto
        {
            Created = written,
            ModelVersion = modelVersion,
            ActualsUpdated = actualsUpdated,
            AlertsCreated = newAlerts.Count
        };
    }

    private static StockAlert NewAlert(
        Product product,
        Warehouse warehouse,
        string alertType,
        decimal currentStock,
        decimal threshold,
        decimal? suggestedOrderQty,
        DemandPrediction? prediction,
        DateTime now)
    {
        return new StockAlert
        {
            StockAlertId = Guid.NewGuid(),
            ProductId = product.ProductId,
            WarehouseId = warehouse.WarehouseId,
            DemandPredictionId = prediction?.DemandPredictionId,
            AlertType = alertType,
            CurrentStock = currentStock,
            Threshold = threshold,
            SuggestedOrderQty = suggestedOrderQty,
            Status = AlertStatuses.Open,
            CreatedAt = now
        };
    }

    private async Task NotifyAsync(List<(StockAlert Alert, Product Product, Warehouse Warehouse)> alerts, CancellationToken cancellationToken)
    {
        if (alerts.Count == 0)
        {
            return;
        }

        var content = EmailTemplates.StockAlerts(alerts
            .Select(item => new StockAlertEmailLine(
                item.Product.Sku,
                item.Product.Name,
                item.Warehouse.Name,
                item.Alert.AlertType,
                item.Alert.CurrentStock,
                item.Alert.Threshold,
                item.Alert.SuggestedOrderQty))
            .ToList());

        foreach (var email in await forecastReadService.GetAlertRecipientEmailsAsync(Permissions.AlertsManage, cancellationToken))
        {
            await EmailQueue.EnqueueAsync(unitOfWork, email, content, cancellationToken);
        }
    }
}
