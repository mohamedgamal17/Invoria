using FluentValidation;
using Invoria.Reporting.Domain.Orders.DebtSummary;

namespace Invoria.Reporting.Endpoints.Orders.Requests;

public sealed class GetCustomerDebtSummaryRequestValidator : AbstractValidator<GetCustomerDebtSummaryRequest>
{
    public GetCustomerDebtSummaryRequestValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .MaximumLength(DebtSummaryTableConsts.CustomerIdMaxLength);
    }
}
