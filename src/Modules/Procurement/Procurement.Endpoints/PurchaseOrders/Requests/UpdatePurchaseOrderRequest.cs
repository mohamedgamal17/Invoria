using FastEndpoints;
using FluentValidation;
using Invoria.Procurement.Domain.PurchaseOrders;

namespace Invoria.Procurement.Endpoints.PurchaseOrders.Requests;

public sealed class UpdatePurchaseOrderRequest
{
    [RouteParam]
    public string Id { get; set; } = string.Empty;

    public string SupplierId { get; set; } = default!;
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public List<PurchaseOrderItemRequest> PurchaseOrderItems { get; set; } = [];
}

public sealed class UpdatePurchaseOrderRequestValidator : AbstractValidator<UpdatePurchaseOrderRequest>
{
    public UpdatePurchaseOrderRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .MaximumLength(PurchaseOrderTableConsts.IdMaxLength);

        RuleFor(x => x.SupplierId)
            .NotEmpty()
            .MaximumLength(PurchaseOrderTableConsts.SupplierIdMaxLength);

        RuleFor(x => x.TaxAmount)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.DiscountAmount)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.PurchaseOrderItems)
            .NotEmpty()
            .WithMessage("Purchase order must contain at least one item.");

        RuleForEach(x => x.PurchaseOrderItems)
            .SetValidator(new PurchaseOrderItemRequestValidator());
    }
}

