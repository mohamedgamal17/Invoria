using FluentValidation;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Invoria.Inventory.Application.Batches.Queries.ListBatches;
using Invoria.Inventory.Contracts.Dtos;
using Invoria.Inventory.Endpoints.Batches.Requests;
using MediatR;

namespace Invoria.Inventory.Endpoints.Batches;

public class ListBatchesEndpoint : EndpointBase<ListBatchesRequest, PagingDto<BatchDto>>
{
    private readonly IMediator _mediator;

    public ListBatchesEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
        : base(resultMapper)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("");
        AllowAnonymous();

        Group<BatchRoutingGroup>();
    }

    public override async Task HandleAsync(ListBatchesRequest req, CancellationToken ct)
    {
        var validator = Resolve<IValidator<ListBatchesRequest>>();
        var validationResult = validator.Validate(req);

        if (!validationResult.IsValid)
        {
            foreach (var failure in validationResult.Errors)
            {
                AddError(failure.PropertyName, failure.ErrorMessage);
            }

            ThrowIfAnyErrors();
        }

        var query = new ListBatchesQuery
        {
            Skip = req.Skip,
            Length = req.Length,
            ProductId = req.ProductId,
            State = req.State
        };

        var result = await _mediator.Send(query, ct);

        await SendResultAsync(result, ct);
    }
}
