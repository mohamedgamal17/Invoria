using Invoria.Reporting.Application.Orders;

namespace Invoria.Reporting.Application.Orders.Queries.ListOrderPeriodSummary;

/// <summary>
/// Resolves optional UTC calendar bounds for <see cref="ListOrderPeriodSummaryQuery"/>.
/// </summary>
public static class ListOrderPeriodSummaryDateRange
{
    public const int DefaultLookbackDays = 30;

    /// <summary>
    /// Both omitted: <c>To</c> = UTC today, <c>From</c> = <c>To</c> − 30 days.
    /// Only <c>from</c> omitted: <c>To</c> = user's end day, <c>From</c> = <c>To</c> − 30 days.
    /// Only <c>to</c> omitted: <c>From</c> = user's start day, <c>To</c> = UTC today.
    /// </summary>
    public static (DateOnly FromDayInclusiveUtc, DateOnly ToDayInclusiveUtc) Resolve(
        DateTime? from,
        DateTime? to,
        DateTime utcNow)
    {
        var today = DateOnly.FromDateTime(utcNow.Kind == DateTimeKind.Utc ? utcNow : utcNow.ToUniversalTime());

        if (from is null && to is null)
        {
            var toDay = today;
            var fromDay = toDay.AddDays(-DefaultLookbackDays);
            return (fromDay, toDay);
        }

        if (from is null && to is not null)
        {
            var toDay = DateOnly.FromDateTime(UtcReportingCalendar.ToUtc(to.Value));
            var fromDay = toDay.AddDays(-DefaultLookbackDays);
            return (fromDay, toDay);
        }

        if (from is not null && to is null)
        {
            var fromDay = DateOnly.FromDateTime(UtcReportingCalendar.ToUtc(from.Value));
            return (fromDay, today);
        }

        var f = DateOnly.FromDateTime(UtcReportingCalendar.ToUtc(from!.Value));
        var t = DateOnly.FromDateTime(UtcReportingCalendar.ToUtc(to!.Value));
        return (f, t);
    }

    public static bool IsValidSpan(DateOnly fromDayInclusiveUtc, DateOnly toDayInclusiveUtc, int maxInclusiveDaySpan)
    {
        if (fromDayInclusiveUtc > toDayInclusiveUtc)
        {
            return false;
        }

        return toDayInclusiveUtc.DayNumber - fromDayInclusiveUtc.DayNumber <= maxInclusiveDaySpan;
    }
}
