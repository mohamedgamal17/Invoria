using FluentValidation;

namespace Invoria.Ordering.Endpoints.Orders.Requests;

public class CreateOrderLineItemRequest
{
    public string ProductId { get; set; } = default!;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

public class CreateOrderLineItemRequestValdiator : AbstractValidator<CreateOrderLineItemRequest>
{
    public CreateOrderLineItemRequestValdiator()
    {
        RuleFor(i => i.ProductId).NotEmpty();
        RuleFor(i => i.Quantity).GreaterThan(0);
        RuleFor(i => i.Price).GreaterThan(0);
    }
}
