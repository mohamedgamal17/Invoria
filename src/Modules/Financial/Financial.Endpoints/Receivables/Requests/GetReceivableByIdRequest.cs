using FluentValidation;
using Invoria.Financial.Domain.Receivables;

namespace Invoria.Financial.Endpoints.Receivables.Requests;

public class GetReceivableByIdRequest
{
    public string Id { get; set; } = string.Empty;
}

public class GetReceivableByIdRequestValidator : AbstractValidator<GetReceivableByIdRequest>
{
    public GetReceivableByIdRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .MaximumLength(ReceivableTableConsts.IdMaxLength);
    }
}
