using FluentValidation;
using Invoria.Financial.Domain.Receivables;

namespace Invoria.Financial.Endpoints.Receivables.Requests;

public class SettleReceivableRequest
{
    public string Id { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class SettleReceivableRequestValidator : AbstractValidator<SettleReceivableRequest>
{
    public SettleReceivableRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .MaximumLength(ReceivableTableConsts.IdMaxLength);

        RuleFor(x => x.Amount)
            .GreaterThan(0m);
    }
}
