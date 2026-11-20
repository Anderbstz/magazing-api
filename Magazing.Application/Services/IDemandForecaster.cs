namespace Magazing.Application.Services;

public interface IDemandForecaster
{
    string ModelVersion { get; }

    int HistoryWeeks { get; }

    IReadOnlyList<WeeklyForecast> Forecast(IReadOnlyList<decimal> weeklyHistory, DateOnly firstWeekStart, int horizonWeeks);
}

public record WeeklyForecast(DateOnly WeekStart, decimal Predicted, decimal Lower, decimal Upper);
