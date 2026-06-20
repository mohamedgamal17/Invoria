using FluentValidation;
using Invoria.Ordering.Domain.Invoices;

namespace Invoria.Ordering.Endpoints.Invoices.Requests;

public class GetInvoiceByIdRequest
{
    public string Id { get; set; } = string.Empty;
}

public class GetInvoiceByIdRequestValidator : AbstractValidator<GetInvoiceByIdRequest>
{
    public GetInvoiceByIdRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .MaximumLength(InvoiceTableConsts.IdMaxLength);
    }
}
