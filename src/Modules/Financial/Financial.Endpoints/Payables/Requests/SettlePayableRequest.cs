using FluentValidation;
using Invoria.Financial.Domain.Payables;

namespace Invoria.Financial.Endpoints.Payables.Requests;

public class SettlePayableRequest
{
    public string Id { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class SettlePayableRequestValidator : AbstractValidator<SettlePayableRequest>
{
    public SettlePayableRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .MaximumLength(PayableTableConsts.IdMaxLength);

        RuleFor(x => x.Amount)
            .GreaterThan(0m);
    }
}
