using FastEndpoints;
using FluentValidation;
using Invoria.BuildingBlocks.Application.Requests;
using Invoria.Procurement.Domain.Parties;

namespace Invoria.Procurement.Endpoints.Parties.Requests;

public class ListSuppliersRequest : PagingParams
{
    [QueryParam]
    public string? Name { get; set; }

    [QueryParam]
    public string? Code { get; set; }
}

public class ListSuppliersRequestValidator : AbstractValidator<ListSuppliersRequest>
{
    public ListSuppliersRequestValidator()
    {
        Include(new PagingParamasValidator<ListSuppliersRequest>());

        When(x => !string.IsNullOrWhiteSpace(x.Name), () =>
        {
            RuleFor(x => x.Name!)
                .MaximumLength(SupplierTableConsts.NameMaxLength);
        });

        When(x => !string.IsNullOrWhiteSpace(x.Code), () =>
        {
            RuleFor(x => x.Code!)
                .MaximumLength(SupplierTableConsts.SupplierCodeMaxLength);
        });
    }
}
