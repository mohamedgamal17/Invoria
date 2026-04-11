using FastEndpoints;
using FluentValidation;

namespace Invoria.Ordering.Endpoints.Orders.Requests;

public class CompleteOrderRequest
{
    [RouteParam]
    public string Id { get; set; } = string.Empty;
}

public class CompleteOrderRequestValidator : AbstractValidator<CompleteOrderRequest>
{
    public CompleteOrderRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}
