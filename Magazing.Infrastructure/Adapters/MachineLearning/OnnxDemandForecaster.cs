using System.Globalization;
using System.Text.Json;
using Magazing.Application.Services;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace Magazing.Infrastructure.Adapters.MachineLearning;

public sealed class OnnxDemandForecaster : IDemandForecaster, IDisposable
{
    private const string ModelFile = "demand-forecast.onnx";
    private const string MetadataFile = "demand-forecast.json";

    private readonly InferenceSession _session;
    private readonly ForecastModelMetadata _metadata;

    public OnnxDemandForecaster()
    {
        var directory = Path.Combine(AppContext.BaseDirectory, "MachineLearning");
        _metadata = JsonSerializer.Deserialize<ForecastModelMetadata>(
                File.ReadAllText(Path.Combine(directory, MetadataFile)),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidOperationException("No se pudo leer la metadata del modelo de demanda.");

        _session = new InferenceSession(Path.Combine(directory, ModelFile));
    }

    public string ModelVersion => _metadata.ModelVersion;

    public int HistoryWeeks => _metadata.HistoryWeeks;

    public IReadOnlyList<WeeklyForecast> Forecast(IReadOnlyList<decimal> weeklyHistory, DateOnly firstWeekStart, int horizonWeeks)
    {
        var window = weeklyHistory.Select(value => (double)value).ToList();
        while (window.Count < HistoryWeeks)
        {
            window.Insert(0, 0);
        }

        var results = new List<WeeklyForecast>();

        for (var step = 0; step < horizonWeeks; step++)
        {
            var weekStart = firstWeekStart.AddDays(7 * step);
            var history = window.Skip(window.Count - HistoryWeeks).ToArray();
            var scale = history.Average() + 1.0;

            var ratio = Predict(BuildFeatures(history, weekStart));
            var predicted = Math.Max(0, ratio * scale);
            var lower = Math.Max(0, predicted + _metadata.ResidualRatioP10 * scale);
            var upper = Math.Max(lower, predicted + _metadata.ResidualRatioP90 * scale);

            results.Add(new WeeklyForecast(weekStart, Round(predicted), Round(lower), Round(upper)));
            window.Add(predicted);
        }

        return results;
    }

    public static float[] BuildFeatures(double[] history, DateOnly targetWeekStart)
    {
        var mean8 = history.Average();
        var mean4 = history.Skip(history.Length - 4).Average();
        var older4 = history.Take(4).Average();
        var scale = mean8 + 1.0;
        var isoWeek = ISOWeek.GetWeekOfYear(targetWeekStart.ToDateTime(TimeOnly.MinValue));
        var angle = 2.0 * Math.PI * isoWeek / 52.0;

        return
        [
            (float)(history[^1] / scale),
            (float)(history[^2] / scale),
            (float)(history[^3] / scale),
            (float)(history[^4] / scale),
            (float)(mean4 / scale),
            (float)Math.Log(1.0 + mean8),
            (float)((mean4 - older4) / scale),
            (float)Math.Sin(angle),
            (float)Math.Cos(angle)
        ];
    }

    private double Predict(float[] features)
    {
        var tensor = new DenseTensor<float>(features, [1, features.Length]);
        using var outputs = _session.Run([NamedOnnxValue.CreateFromTensor(_metadata.InputName, tensor)]);
        return outputs[0].AsEnumerable<float>().First();
    }

    private static decimal Round(double value)
    {
        return Math.Round((decimal)value, 2);
    }

    public void Dispose()
    {
        _session.Dispose();
    }

    private sealed class ForecastModelMetadata
    {
        public string ModelVersion { get; set; } = null!;

        public int HistoryWeeks { get; set; }

        public string InputName { get; set; } = null!;

        public double ResidualRatioP10 { get; set; }

        public double ResidualRatioP90 { get; set; }
    }
}
