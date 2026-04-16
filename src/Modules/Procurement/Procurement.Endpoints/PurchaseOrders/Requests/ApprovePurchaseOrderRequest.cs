using FastEndpoints;
using FluentValidation;
using Invoria.Procurement.Domain.PurchaseOrders;

namespace Invoria.Procurement.Endpoints.PurchaseOrders.Requests;

public sealed class ApprovePurchaseOrderRequest
{
    [RouteParam]
    public string Id { get; set; } = string.Empty;
}

public sealed class ApprovePurchaseOrderRequestValidator : AbstractValidator<ApprovePurchaseOrderRequest>
{
    public ApprovePurchaseOrderRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .MaximumLength(PurchaseOrderTableConsts.IdMaxLength);
    }
}

