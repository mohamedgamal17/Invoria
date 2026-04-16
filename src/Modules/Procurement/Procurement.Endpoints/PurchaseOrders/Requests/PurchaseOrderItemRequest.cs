using FluentValidation;
using Invoria.Procurement.Domain.PurchaseOrders;

namespace Invoria.Procurement.Endpoints.PurchaseOrders.Requests;

public sealed class PurchaseOrderItemRequest
{
    public string ProductId { get; set; } = default!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? SupplierProductCode { get; set; }
}

public sealed class PurchaseOrderItemRequestValidator : AbstractValidator<PurchaseOrderItemRequest>
{
    public PurchaseOrderItemRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .MaximumLength(PurchaseOrderItemTableConsts.ProductIdMaxLength);

        RuleFor(x => x.Quantity)
            .GreaterThan(0);

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.SupplierProductCode)
            .MaximumLength(PurchaseOrderItemTableConsts.SupplierProductCodeMaxLength)
            .When(x => !string.IsNullOrWhiteSpace(x.SupplierProductCode));
    }
}
