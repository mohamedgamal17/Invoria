using FluentValidation;

namespace Invoria.Ordering.Endpoints.Orders.Requests;

public class DispatchOrderRequest
{
    public string Id { get; set; } = string.Empty;
}

public class DispatchOrderRequestValidator : AbstractValidator<DispatchOrderRequest>
{
    public DispatchOrderRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}
