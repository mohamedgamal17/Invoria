using FluentValidation;

namespace Invoria.Ordering.Endpoints.Orders.Requests;

public class GetOrderByIdRequest
{
    public string Id { get; set; } = string.Empty;
}

public class GetOrderByIdRequestValidator : AbstractValidator<GetOrderByIdRequest>
{
    public GetOrderByIdRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}
