using FastEndpoints;
using FluentValidation;
using Invoria.Ordering.Domain.Orders;

namespace Invoria.Ordering.Endpoints.Orders.Requests;

public class AddReturnItemsRequest
{
    [RouteParam]
    public string Id { get; set; } = string.Empty;

    public List<AddReturnLineItemRequest> Items { get; set; } = new();
}

public class AddReturnLineItemRequest
{
    public string OrderItemId { get; set; } = string.Empty;

    public int Quantity { get; set; }
}

public class AddReturnItemsRequestValidator : AbstractValidator<AddReturnItemsRequest>
{
    public AddReturnItemsRequestValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleForEach(x => x.Items).SetValidator(new AddReturnLineItemRequestValidator());
    }
}

public class AddReturnLineItemRequestValidator : AbstractValidator<AddReturnLineItemRequest>
{
    public AddReturnLineItemRequestValidator()
    {
        RuleFor(x => x.OrderItemId)
            .NotEmpty()
            .MaximumLength(OrderItemTableConsts.IdMaxLength);

        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
