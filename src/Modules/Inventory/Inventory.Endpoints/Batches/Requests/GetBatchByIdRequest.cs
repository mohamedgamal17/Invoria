using FluentValidation;

namespace Invoria.Inventory.Endpoints.Batches.Requests;

public class GetBatchByIdRequest
{
    public string Id { get; set; } = string.Empty;
}

public class GetBatchByIdRequestValidator : AbstractValidator<GetBatchByIdRequest>
{
    public GetBatchByIdRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}
