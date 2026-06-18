using FluentValidation;
using Invoria.Ordering.Contracts.Orders.Enums;

namespace Invoria.Ordering.Endpoints.Orders.Requests;

public class CreateOrderRequest
{
    public string CustomerId { get; set; } = default!;
    public List<CreateOrderLineItemRequest> Items { get; set; } = new();

    /// <summary>When omitted, the order is immediate payment (cash sale).</summary>
    public OrderPaymentType? PaymentType { get; set; }
}

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("Order must contain at least one line item.");

        RuleForEach(x => x.Items).SetValidator(new CreateOrderLineItemRequestValdiator());

        RuleFor(x => x.PaymentType)
            .Must(v => !v.HasValue || Enum.IsDefined(typeof(OrderPaymentType), v.Value));
    }
}
