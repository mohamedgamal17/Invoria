using FastEndpoints;
using FluentValidation;
using Invoria.Ordering.Domain.Orders;

namespace Invoria.Ordering.Endpoints.Orders.Requests;

public class CompleteOrderRequest
{
    [RouteParam]
    public string Id { get; set; } = string.Empty;

    public List<CompleteReturnLineItemRequest>? Items { get; set; }
}

public class CompleteReturnLineItemRequest
{
    public string OrderItemId { get; set; } = string.Empty;

    public int Quantity { get; set; }
}

public class CompleteOrderRequestValidator : AbstractValidator<CompleteOrderRequest>
{
    public CompleteOrderRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleForEach(x => x.Items).SetValidator(new CompleteReturnLineItemRequestValidator());
    }
}

public class CompleteReturnLineItemRequestValidator : AbstractValidator<CompleteReturnLineItemRequest>
{
    public CompleteReturnLineItemRequestValidator()
    {
        RuleFor(x => x.OrderItemId)
            .NotEmpty()
            .MaximumLength(OrderItemTableConsts.IdMaxLength);

        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
