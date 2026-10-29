using FluentValidation;
using Invoria.Financial.Domain.Payables;

namespace Invoria.Financial.Endpoints.Payables.Requests;

public class GetPayableByPurchaseOrderIdRequest
{
    public string PurchaseOrderId { get; set; } = string.Empty;
}

public class GetPayableByPurchaseOrderIdRequestValidator : AbstractValidator<GetPayableByPurchaseOrderIdRequest>
{
    public GetPayableByPurchaseOrderIdRequestValidator()
    {
        RuleFor(x => x.PurchaseOrderId)
            .NotEmpty()
            .MaximumLength(PayableTableConsts.SourceIdMaxLength);
    }
}
