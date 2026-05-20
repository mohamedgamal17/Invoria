using FluentValidation;

namespace Invoria.Reporting.Endpoints.Orders.Requests;

public sealed class GetOrderStatusSummaryRequestValidator : AbstractValidator<GetOrderStatusSummaryRequest>
{
    public const int MaxInclusiveDaySpan = 366;

    public GetOrderStatusSummaryRequestValidator()
    {
        RuleFor(x => x)
            .Must(x => x.FromUtc.HasValue == x.ToUtc.HasValue)
            .WithMessage("Provide both FromUtc and ToUtc, or omit both.");

        RuleFor(x => x)
            .Must(x =>
                x.FromUtc is not null &&
                x.ToUtc is not null &&
                x.FromUtc.Value.UtcDateTime <= x.ToUtc.Value.UtcDateTime)
            .WithMessage("FromUtc must be less than or equal to ToUtc (UTC).")
            .When(x => x.FromUtc is not null && x.ToUtc is not null);

        RuleFor(x => x)
            .Must(x =>
            {
                if (x.FromUtc is null || x.ToUtc is null)
                {
                    return true;
                }

                var fromDay = DateOnly.FromDateTime(x.FromUtc.Value.UtcDateTime);
                var toDay = DateOnly.FromDateTime(x.ToUtc.Value.UtcDateTime);
                return toDay.DayNumber - fromDay.DayNumber <= MaxInclusiveDaySpan;
            })
            .WithMessage($"The date range must not span more than {MaxInclusiveDaySpan} calendar days (UTC).")
            .When(x => x.FromUtc is not null && x.ToUtc is not null);
    }
}
