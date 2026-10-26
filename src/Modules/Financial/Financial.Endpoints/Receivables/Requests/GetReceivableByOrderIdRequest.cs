using FluentValidation;
using Invoria.Financial.Domain.Receivables;

namespace Invoria.Financial.Endpoints.Receivables.Requests;

public class GetReceivableByOrderIdRequest
{
    public string OrderId { get; set; } = string.Empty;
}

public class GetReceivableByOrderIdRequestValidator : AbstractValidator<GetReceivableByOrderIdRequest>
{
    public GetReceivableByOrderIdRequestValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .MaximumLength(ReceivableTableConsts.SourceIdMaxLength);
    }
}
