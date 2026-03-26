using FluentValidation;

namespace Invoria.Ordering.Endpoints.Orders.Requests;

public class CancelOrderRequest
{
    public string Id { get; set; } = string.Empty;
}

public class CancelOrderRequestValidator : AbstractValidator<CancelOrderRequest>
{
    public CancelOrderRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}

