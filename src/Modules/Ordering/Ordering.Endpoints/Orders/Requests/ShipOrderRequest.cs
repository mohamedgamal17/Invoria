using FluentValidation;

namespace Invoria.Ordering.Endpoints.Orders.Requests;

public class ShipOrderRequest
{
    public string Id { get; set; } = string.Empty;
}

public class ShipOrderRequestValidator : AbstractValidator<ShipOrderRequest>
{
    public ShipOrderRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}
