using FluentValidation;

namespace Invoria.Ordering.Endpoints.Orders.Requests;

public class RefuseOrderRequest
{
    public string Id { get; set; } = string.Empty;
}

public class RefuseOrderRequestValidator : AbstractValidator<RefuseOrderRequest>
{
    public RefuseOrderRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}

