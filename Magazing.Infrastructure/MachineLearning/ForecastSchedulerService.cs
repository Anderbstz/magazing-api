using Magazing.Application.UseCases.Predictions.Commands;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Magazing.Infrastructure.MachineLearning;

public class ForecastSchedulerService(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<ForecastSchedulerService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!configuration.GetValue("Forecast:SchedulerEnabled", true))
        {
            logger.LogInformation("La ejecucion programada de predicciones esta deshabilitada.");
            return;
        }

        var interval = TimeSpan.FromHours(Math.Max(configuration.GetValue("Forecast:RunIntervalHours", 24), 1));
        await Task.Delay(TimeSpan.FromMinutes(Math.Max(configuration.GetValue("Forecast:StartupDelayMinutes", 2), 0)), stoppingToken);

        using var timer = new PeriodicTimer(interval);

        do
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var result = await scope.ServiceProvider.GetRequiredService<ISender>().Send(new RunPredictionsCommand(), stoppingToken);
                logger.LogInformation(
                    "Predicciones {Version}: {Created} semanas, {Alerts} alertas nuevas.",
                    result.ModelVersion,
                    result.Created,
                    result.AlertsCreated);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Error al ejecutar las predicciones programadas.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
