using Invoria.Reporting.Endpoints.Orders.Requests;

namespace Invoria.Reporting.Application.Tests.Orders.Requests;

[TestFixture]
public sealed class ListOrderPeriodSummaryRequestValidatorTests
{
    private readonly ListOrderPeriodSummaryRequestValidator _validator = new();

    [Test]
    public void Passes_when_from_and_to_omitted()
    {
        var req = new ListOrderPeriodSummaryRequest
        {
            From = null,
            To = null,
            Skip = 0,
            Length = 10
        };

        var result = _validator.Validate(req);

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Passes_when_to_omitted_and_from_within_span_of_today()
    {
        var req = new ListOrderPeriodSummaryRequest
        {
            From = DateTime.UtcNow.AddDays(-10),
            To = null,
            Skip = 0,
            Length = 10
        };

        var result = _validator.Validate(req);

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Passes_when_from_omitted_and_to_is_today_window()
    {
        var req = new ListOrderPeriodSummaryRequest
        {
            From = null,
            To = DateTime.UtcNow.Date,
            Skip = 0,
            Length = 10
        };

        var result = _validator.Validate(req);

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Fails_when_from_calendar_day_is_after_to_calendar_day()
    {
        var req = new ListOrderPeriodSummaryRequest
        {
            From = new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc),
            To = new DateTime(2026, 5, 9, 0, 0, 0, DateTimeKind.Utc),
            Skip = 0,
            Length = 10
        };

        var result = _validator.Validate(req);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.ErrorMessage == "From date must be before To date."), Is.True);
    }

    [Test]
    public void Fails_when_span_exceeds_366_days()
    {
        var from = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = from.AddDays(367);
        var req = new ListOrderPeriodSummaryRequest { From = from, To = to, Skip = 0, Length = 10 };

        var result = _validator.Validate(req);

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors.Any(e => e.ErrorMessage == "Date range cannot exceed 366 days."), Is.True);
    }

    [Test]
    public void Fails_when_length_is_zero()
    {
        var req = new ListOrderPeriodSummaryRequest
        {
            From = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc),
            To = new DateTime(2026, 5, 2, 0, 0, 0, DateTimeKind.Utc),
            Skip = 0,
            Length = 0
        };

        var result = _validator.Validate(req);

        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public void Fails_when_length_exceeds_200()
    {
        var req = new ListOrderPeriodSummaryRequest
        {
            From = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc),
            To = new DateTime(2026, 5, 2, 0, 0, 0, DateTimeKind.Utc),
            Skip = 0,
            Length = 201
        };

        var result = _validator.Validate(req);

        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public void Passes_for_same_day_range()
    {
        var req = new ListOrderPeriodSummaryRequest
        {
            From = new DateTime(2026, 5, 1, 10, 0, 0, DateTimeKind.Utc),
            To = new DateTime(2026, 5, 1, 18, 0, 0, DateTimeKind.Utc),
            Skip = 0,
            Length = 50
        };

        var result = _validator.Validate(req);

        Assert.That(result.IsValid, Is.True);
    }
}
