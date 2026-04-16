using FastEndpoints;
using FluentValidation;
using Invoria.Procurement.Domain.PurchaseOrders;

namespace Invoria.Procurement.Endpoints.PurchaseOrders.Requests;

public sealed class CompletePurchaseOrderRequest
{
    [RouteParam]
    public string Id { get; set; } = string.Empty;
}

public sealed class CompletePurchaseOrderRequestValidator : AbstractValidator<CompletePurchaseOrderRequest>
{
    public CompletePurchaseOrderRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .MaximumLength(PurchaseOrderTableConsts.IdMaxLength);
    }
}

