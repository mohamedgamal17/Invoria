using FluentValidation;

namespace Invoria.Ordering.Endpoints.Orders.Requests;

public class AcceptOrderRequest
{
    public string Id { get; set; } = string.Empty;
}

public class AcceptOrderRequestValidator : AbstractValidator<AcceptOrderRequest>
{
    public AcceptOrderRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}
