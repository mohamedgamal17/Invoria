using FluentValidation;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Invoria.Inventory.Application.Batches.Commands.CreateBatch;
using Invoria.Inventory.Contracts.Dtos;
using Invoria.Inventory.Endpoints.Batches.Requests;
using MediatR;

namespace Invoria.Inventory.Endpoints.Batches;

public class CreateBatchEndpoint : EndpointBase<CreateBatchRequest, BatchDto>
{
    private readonly IMediator _mediator;

    public CreateBatchEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
        : base(resultMapper)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("");
        AllowAnonymous();

        Group<BatchRoutingGroup>();
    }

    public override async Task HandleAsync(CreateBatchRequest req, CancellationToken ct)
    {
        ValidateRequest(req);

        var command = new CreateBatchCommand(req.ProductId, req.Quantity, req.PurchasePrice);
        var result = await _mediator.Send(command, ct);

        await SendResultAsync(result, ct);
    }
}
