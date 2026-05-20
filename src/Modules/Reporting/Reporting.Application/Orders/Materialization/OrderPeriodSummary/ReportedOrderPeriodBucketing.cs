using Invoria.Reporting.Contracts.Orders.Reports;

namespace Invoria.Reporting.Application.Orders.Materialization.OrderPeriodSummary;

/// <summary>
/// UTC calendar bucketing for materialized order period summaries (placed instant / <see cref="Invoria.Reporting.Domain.Orders.ReportedOrder.CreatedAt"/> only).
/// </summary>
public static class ReportedOrderPeriodBucketing
{
    public const string GranularityDay = nameof(OrderPeriodSummaryGranularity.Day);
    public const string GranularityWeek = nameof(OrderPeriodSummaryGranularity.Week);
    public const string GranularityMonth = nameof(OrderPeriodSummaryGranularity.Month);

    public static PeriodBucket? GetBucket(DateTimeOffset effectiveUtc, string granularity)
    {
        var utc = effectiveUtc.UtcDateTime;
        return granularity switch
        {
            GranularityDay => DayBucket(utc),
            GranularityWeek => WeekBucket(utc),
            GranularityMonth => MonthBucket(utc),
            _ => null
        };
    }

    public static PeriodBucket DayBucket(DateTime utc)
    {
        var d = DateOnly.FromDateTime(utc);
        var key = d.ToString("yyyy-MM-dd");
        return new PeriodBucket(key, d, d);
    }

    public static PeriodBucket WeekBucket(DateTime utc)
    {
        var d = DateOnly.FromDateTime(utc);
        var daysSinceMonday = ((int)d.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        var monday = d.AddDays(-daysSinceMonday);
        var sunday = monday.AddDays(6);
        var key = $"W/c {monday:yyyy-MM-dd}";
        return new PeriodBucket(key, monday, sunday);
    }

    public static PeriodBucket MonthBucket(DateTime utc)
    {
        var d = DateOnly.FromDateTime(utc);
        var start = new DateOnly(d.Year, d.Month, 1);
        var end = start.AddMonths(1).AddDays(-1);
        var key = utc.ToString("yyyy-MM");
        return new PeriodBucket(key, start, end);
    }

    public static IReadOnlyList<string> AllGranularities { get; } = new[] { GranularityDay, GranularityWeek, GranularityMonth };
}

public readonly record struct PeriodBucket(string PeriodKey, DateOnly PeriodStart, DateOnly PeriodEnd);
