using FastEndpoints;
using FluentValidation;
using Invoria.BuildingBlocks.Application.Requests;
using Invoria.Financial.Domain.Receivables;

namespace Invoria.Financial.Endpoints.Receivables.Requests;

public class ListReceivablesRequest : PagingParams
{
    [QueryParam]
    public string? PartyId { get; set; }

    [QueryParam]
    public string? SourceId { get; set; }

    [QueryParam]
    public bool? IsPaid { get; set; }
}

public class ListReceivablesRequestValidator : AbstractValidator<ListReceivablesRequest>
{
    public ListReceivablesRequestValidator()
    {
        Include(new PagingParamasValidator<ListReceivablesRequest>());

        When(x => x.PartyId is not null, () =>
        {
            RuleFor(x => x.PartyId!)
                .NotEmpty()
                .MaximumLength(ReceivableTableConsts.PartyIdMaxLength);
        });

        When(x => x.SourceId is not null, () =>
        {
            RuleFor(x => x.SourceId!)
                .NotEmpty()
                .MaximumLength(ReceivableTableConsts.SourceIdMaxLength);
        });
    }
}
