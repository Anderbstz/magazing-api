namespace Magazing.Application.Common;

public static class BusinessTime
{
    private static readonly TimeSpan LimaOffset = TimeSpan.FromHours(-5);

    public static DateOnly Today()
    {
        return DateOnly.FromDateTime(DateTime.UtcNow.Add(LimaOffset));
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
