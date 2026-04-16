using FastEndpoints;
using FluentValidation;
using Invoria.Procurement.Domain.PurchaseOrders;

namespace Invoria.Procurement.Endpoints.PurchaseOrders.Requests;

public sealed class ReopenPurchaseOrderRequest
{
    [RouteParam]
    public string Id { get; set; } = string.Empty;
}

public sealed class ReopenPurchaseOrderRequestValidator : AbstractValidator<ReopenPurchaseOrderRequest>
{
    public ReopenPurchaseOrderRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .MaximumLength(PurchaseOrderTableConsts.IdMaxLength);
    }
}
