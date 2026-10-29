using FluentValidation;
using Invoria.Financial.Domain.Payables;

namespace Invoria.Financial.Endpoints.Payables.Requests;

public class GetPayableByIdRequest
{
    public string Id { get; set; } = string.Empty;
}

public class GetPayableByIdRequestValidator : AbstractValidator<GetPayableByIdRequest>
{
    public GetPayableByIdRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .MaximumLength(PayableTableConsts.IdMaxLength);
    }
}
