using FluentValidation;

namespace Invoria.Inventory.Endpoints.Returns.Requests;

public class GetReturnByIdRequest
{
    public string Id { get; set; } = string.Empty;
}

public class GetReturnByIdRequestValidator : AbstractValidator<GetReturnByIdRequest>
{
    public GetReturnByIdRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}
