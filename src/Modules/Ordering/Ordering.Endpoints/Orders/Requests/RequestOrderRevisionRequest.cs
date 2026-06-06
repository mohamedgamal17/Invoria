using FastEndpoints;
using FluentValidation;

namespace Invoria.Ordering.Endpoints.Orders.Requests;

public class RequestOrderRevisionRequest
{
    [RouteParam]
    public string Id { get; set; } = string.Empty;
}

public class RequestOrderRevisionRequestValidator : AbstractValidator<RequestOrderRevisionRequest>
{
    public RequestOrderRevisionRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}
