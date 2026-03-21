using FluentValidation;

namespace Invoria.Ordering.Endpoints.Orders.Requests;

public class CreateOrderRequest
{
    public string CustomerId { get; set; } = default!;
    public List<CreateOrderLineItemRequest> Items { get; set; } = new();
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
    }
}
