using FluentValidation;

namespace Invoria.Ordering.Endpoints.Orders.Requests;

public class ReopenOrderRequest
{
    public string Id { get; set; } = string.Empty;
}

public class ReopenOrderRequestValidator : AbstractValidator<ReopenOrderRequest>
{
    public ReopenOrderRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}
