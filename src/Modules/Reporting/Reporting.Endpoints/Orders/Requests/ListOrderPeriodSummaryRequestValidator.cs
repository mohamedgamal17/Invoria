using FluentValidation;
using Invoria.BuildingBlocks.Application.Requests;
using Invoria.Reporting.Application.Orders.Queries.ListOrderPeriodSummary;

namespace Invoria.Reporting.Endpoints.Orders.Requests;

public sealed class ListOrderPeriodSummaryRequestValidator : AbstractValidator<ListOrderPeriodSummaryRequest>
{
    public const int MaxInclusiveDaySpan = 366;

    public ListOrderPeriodSummaryRequestValidator()
    {
        Include(new PagingParamasValidator<ListOrderPeriodSummaryRequest>());

        RuleFor(x => x)
            .Must(x =>
            {
                var (fromDay, toDay) = ListOrderPeriodSummaryDateRange.Resolve(x.From, x.To, DateTime.UtcNow);
                return fromDay <= toDay;
            })
            .WithMessage("From date must be before To date.");

        RuleFor(x => x)
            .Must(x =>
            {
                var (fromDay, toDay) = ListOrderPeriodSummaryDateRange.Resolve(x.From, x.To, DateTime.UtcNow);
                return ListOrderPeriodSummaryDateRange.IsValidSpan(fromDay, toDay, MaxInclusiveDaySpan);
            })
            .WithMessage("Date range cannot exceed 366 days.");
    }
}
