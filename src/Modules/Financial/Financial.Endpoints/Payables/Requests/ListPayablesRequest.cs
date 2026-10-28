using FastEndpoints;
using FluentValidation;
using Invoria.BuildingBlocks.Application.Requests;
using Invoria.Financial.Domain.Payables;

namespace Invoria.Financial.Endpoints.Payables.Requests;

public class ListPayablesRequest : PagingParams
{
    [QueryParam]
    public string? PartyId { get; set; }

    [QueryParam]
    public string? SourceId { get; set; }

    [QueryParam]
    public bool? IsPaid { get; set; }
}

public class ListPayablesRequestValidator : AbstractValidator<ListPayablesRequest>
{
    public ListPayablesRequestValidator()
    {
        Include(new PagingParamasValidator<ListPayablesRequest>());

        When(x => x.PartyId is not null, () =>
        {
            RuleFor(x => x.PartyId!)
                .NotEmpty()
                .MaximumLength(PayableTableConsts.PartyIdMaxLength);
        });

        When(x => x.SourceId is not null, () =>
        {
            RuleFor(x => x.SourceId!)
                .NotEmpty()
                .MaximumLength(PayableTableConsts.SourceIdMaxLength);
        });
    }
}
