using FluentValidation;

namespace Invoria.Ordering.Endpoints.Orders.Requests;

public class UpdateOrderItemsRequest
{
    public string Id { get; set; } = string.Empty;
    public List<CreateOrderLineItemRequest> Items { get; set; } = new();
}

public class UpdateOrderItemsRequestValidator : AbstractValidator<UpdateOrderItemsRequest>
{
    public UpdateOrderItemsRequestValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("Order must contain at least one line item.");

        RuleForEach(x => x.Items).SetValidator(new CreateOrderLineItemRequestValdiator());
    }
}
