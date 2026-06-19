using FastEndpoints;
using FluentValidation;
using Invoria.BuildingBlocks.Application.Requests;
using Invoria.Ordering.Domain.Invoices;

namespace Invoria.Ordering.Endpoints.Invoices.Requests;

public class ListInvoicesRequest : PagingParams
{
    [QueryParam]
    public string? CustomerId { get; set; }

    [QueryParam]
    public string? OrderId { get; set; }
}

public class ListInvoicesRequestValidator : AbstractValidator<ListInvoicesRequest>
{
    public ListInvoicesRequestValidator()
    {
        Include(new PagingParamasValidator<ListInvoicesRequest>());

        When(x => x.CustomerId is not null, () =>
        {
            RuleFor(x => x.CustomerId!)
                .NotEmpty()
                .MaximumLength(InvoiceTableConsts.CustomerIdMaxLength);
        });

        When(x => x.OrderId is not null, () =>
        {
            RuleFor(x => x.OrderId!)
                .NotEmpty()
                .MaximumLength(InvoiceTableConsts.OrderIdMaxLength);
        });
    }
}
