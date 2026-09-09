using FluentValidation;
using Invoria.Procurement.Domain.PurchaseOrders;

namespace Invoria.Procurement.Endpoints.PurchaseOrders.Requests;

public sealed class CreatePurchaseOrderRequest
{
    public string SupplierId { get; set; } = default!;
    public List<PurchaseOrderItemRequest> PurchaseOrderItems { get; set; } = [];
}

public sealed class CreatePurchaseOrderRequestValidator : AbstractValidator<CreatePurchaseOrderRequest>
{
    public CreatePurchaseOrderRequestValidator()
    {
        RuleFor(x => x.SupplierId)
            .NotEmpty()
            .MaximumLength(PurchaseOrderTableConsts.SupplierIdMaxLength);

        RuleFor(x => x.PurchaseOrderItems)
            .NotEmpty()
            .WithMessage("Purchase order must contain at least one item.");

        RuleForEach(x => x.PurchaseOrderItems)
            .SetValidator(new PurchaseOrderItemRequestValidator());
    }
}
