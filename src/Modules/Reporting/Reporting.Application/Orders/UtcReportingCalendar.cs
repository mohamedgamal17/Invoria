namespace Invoria.Reporting.Application.Orders;

public static class UtcReportingCalendar
{
    public static DateTime ToUtc(DateTime dt) =>
        dt.Kind switch
        {
            DateTimeKind.Utc => dt,
            DateTimeKind.Local => dt.ToUniversalTime(),
            _ => DateTime.SpecifyKind(dt, DateTimeKind.Utc)
        };

    public static DateTimeOffset StartOfUtcDay(DateOnly day) =>
        new(day.Year, day.Month, day.Day, 0, 0, 0, TimeSpan.Zero);

    public static DateTimeOffset EndOfUtcDay(DateOnly day) =>
        new(day.Year, day.Month, day.Day, 23, 59, 59, 999, TimeSpan.Zero);
}
