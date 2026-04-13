using FluentValidation;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Invoria.Inventory.Application.Batches.Commands.UpdateBatch;
using Invoria.Inventory.Contracts.Dtos;
using Invoria.Inventory.Endpoints.Batches.Requests;
using MediatR;

namespace Invoria.Inventory.Endpoints.Batches;

public class UpdateBatchEndpoint : EndpointBase<UpdateBatchRequest, BatchDto>
{
    private readonly IMediator _mediator;

    public UpdateBatchEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
        : base(resultMapper)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Put("{id}");
        AllowAnonymous();

        Group<BatchRoutingGroup>();
    }

    public override async Task HandleAsync(UpdateBatchRequest req, CancellationToken ct)
    {
        ValidateRequest(req);

        var command = new UpdateBatchCommand(req.Id, req.Quantity, req.PurchasePrice);
        var result = await _mediator.Send(command, ct);

        await SendResultAsync(result, ct);
    }
}
