namespace Magazing.Application.Services.Common;

public static class BusinessTime
{
    private static readonly TimeSpan LimaOffset = TimeSpan.FromHours(-5);

    public static DateTime ToLima(DateTime utc)
    {
        return utc.Add(LimaOffset);
    }

    public static DateOnly Today()
    {
        return DateOnly.FromDateTime(DateTime.UtcNow.Add(LimaOffset));
    }

    public static DateOnly WeekStart(DateOnly date)
    {
        return date.AddDays(-(((int)date.DayOfWeek + 6) % 7));
    }

    public static DateOnly WeekStartOf(DateTime utc)
    {
        return WeekStart(DateOnly.FromDateTime(ToLima(utc)));
    }

    public static DateTime StartOfDayUtc(DateOnly date)
    {
        return new DateTimeOffset(date.ToDateTime(TimeOnly.MinValue), LimaOffset).UtcDateTime;
    }

    public static DateTime StartOfNextDayUtc(DateOnly date)
    {
        return StartOfDayUtc(date.AddDays(1));
    }
}
