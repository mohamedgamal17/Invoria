using FastEndpoints;
using FluentValidation;
using Invoria.Procurement.Domain.PurchaseOrders;

namespace Invoria.Procurement.Endpoints.PurchaseOrders.Requests;

public sealed class CancelPurchaseOrderRequest
{
    [RouteParam]
    public string Id { get; set; } = string.Empty;
}

public sealed class CancelPurchaseOrderRequestValidator : AbstractValidator<CancelPurchaseOrderRequest>
{
    public CancelPurchaseOrderRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .MaximumLength(PurchaseOrderTableConsts.IdMaxLength);
    }
}
