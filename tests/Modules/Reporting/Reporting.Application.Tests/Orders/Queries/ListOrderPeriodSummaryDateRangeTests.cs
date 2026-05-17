using Invoria.Reporting.Application.Orders.Queries.ListOrderPeriodSummary;

namespace Invoria.Reporting.Application.Tests.Orders.Queries;

[TestFixture]
public sealed class ListOrderPeriodSummaryDateRangeTests
{
    private static readonly DateTime UtcAnchor = new(2026, 5, 15, 14, 0, 0, DateTimeKind.Utc);

    [Test]
    public void Resolve_both_null_uses_last_30_days_ending_today_utc()
    {
        var (from, to) = ListOrderPeriodSummaryDateRange.Resolve(null, null, UtcAnchor);

        Assert.That(to, Is.EqualTo(new DateOnly(2026, 5, 15)));
        Assert.That(from, Is.EqualTo(new DateOnly(2026, 4, 15)));
    }

    [Test]
    public void Resolve_from_null_uses_to_minus_30_days()
    {
        var toDt = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        var (from, to) = ListOrderPeriodSummaryDateRange.Resolve(null, toDt, UtcAnchor);

        Assert.That(to, Is.EqualTo(new DateOnly(2026, 6, 1)));
        Assert.That(from, Is.EqualTo(new DateOnly(2026, 5, 2)));
    }

    [Test]
    public void Resolve_to_null_uses_from_and_today_utc()
    {
        var fromDt = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc);
        var (from, to) = ListOrderPeriodSummaryDateRange.Resolve(fromDt, null, UtcAnchor);

        Assert.That(from, Is.EqualTo(new DateOnly(2026, 5, 1)));
        Assert.That(to, Is.EqualTo(new DateOnly(2026, 5, 15)));
    }

    [Test]
    public void IsValidSpan_false_when_from_after_to()
    {
        var ok = ListOrderPeriodSummaryDateRange.IsValidSpan(
            new DateOnly(2026, 5, 2),
            new DateOnly(2026, 5, 1),
            366);

        Assert.That(ok, Is.False);
    }

    [Test]
    public void IsValidSpan_false_when_span_exceeds_max()
    {
        var from = new DateOnly(2026, 1, 1);
        var to = from.AddDays(367);
        var ok = ListOrderPeriodSummaryDateRange.IsValidSpan(from, to, 366);

        Assert.That(ok, Is.False);
    }
}
