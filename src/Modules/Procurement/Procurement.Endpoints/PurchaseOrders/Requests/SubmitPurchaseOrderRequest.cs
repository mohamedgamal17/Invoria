using FastEndpoints;
using FluentValidation;
using Invoria.Procurement.Domain.PurchaseOrders;

namespace Invoria.Procurement.Endpoints.PurchaseOrders.Requests;

public sealed class SubmitPurchaseOrderRequest
{
    [RouteParam]
    public string Id { get; set; } = string.Empty;
}

public sealed class SubmitPurchaseOrderRequestValidator : AbstractValidator<SubmitPurchaseOrderRequest>
{
    public SubmitPurchaseOrderRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .MaximumLength(PurchaseOrderTableConsts.IdMaxLength);
    }
}
