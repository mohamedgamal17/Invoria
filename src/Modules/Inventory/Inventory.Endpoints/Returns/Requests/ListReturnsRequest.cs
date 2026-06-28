using FluentValidation;
using Invoria.BuildingBlocks.Application.Requests;
using Invoria.Inventory.Domain.Returns;

namespace Invoria.Inventory.Endpoints.Returns.Requests;

public class ListReturnsRequest : PagingParams
{
    public ReturnType? Type { get; set; }
}

public class ListReturnsRequestValidator : AbstractValidator<ListReturnsRequest>
{
    public ListReturnsRequestValidator()
    {
        Include(new PagingParamasValidator<ListReturnsRequest>());

        RuleFor(x => x.Type)
            .IsInEnum()
            .When(x => x.Type.HasValue);
    }
}
