using FluentValidation;
using Invoria.BuildingBlocks.Application.Requests;
using Invoria.Inventory.Domain.Batches;

namespace Invoria.Inventory.Endpoints.Batches.Requests;

public class ListBatchesRequest : PagingParams
{
    public string? ProductId { get; set; }
    public BatchState? State { get; set; }
}

public class ListBatchesRequestValidator : AbstractValidator<ListBatchesRequest>
{
    public ListBatchesRequestValidator()
    {
        Include(new PagingParamasValidator<ListBatchesRequest>());

        When(x => x.ProductId is not null, () =>
        {
            RuleFor(x => x.ProductId!)
                .NotEmpty()
                .MaximumLength(BatchTableConsts.ProductIdMaxLength);
        });

        RuleFor(x => x.State)
            .IsInEnum()
            .When(x => x.State.HasValue);
    }
}
