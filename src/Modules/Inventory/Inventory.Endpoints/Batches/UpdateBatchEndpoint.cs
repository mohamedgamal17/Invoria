using FluentValidation;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.OpenApi;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Microsoft.AspNetCore.Http;
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

        Summary(s =>
        {
            s.Summary = "Update batch";
            s.Description = "Updates quantity and purchase price for an existing batch.";
            s.Responses[StatusCodes.Status200OK] =
                InvoriaOpenApiResponseDescriptions.Ok200 + " Returns the updated batch.";
            s.Responses[StatusCodes.Status400BadRequest] = InvoriaOpenApiResponseDescriptions.BadRequest400;
            s.Responses[StatusCodes.Status404NotFound] = InvoriaOpenApiResponseDescriptions.NotFound404;
            s.Responses[StatusCodes.Status409Conflict] = InvoriaOpenApiResponseDescriptions.Conflict409;
            s.Responses[StatusCodes.Status422UnprocessableEntity] = InvoriaOpenApiResponseDescriptions.UnprocessableEntity422;
            s.Responses[StatusCodes.Status500InternalServerError] = InvoriaOpenApiResponseDescriptions.InternalServerError500;
        });
    }

    public override async Task HandleAsync(UpdateBatchRequest req, CancellationToken ct)
    {
        ValidateRequest(req);

        var command = new UpdateBatchCommand(req.Id, req.Quantity, req.PurchasePrice);
        var result = await _mediator.Send(command, ct);

        await SendResultAsync(result, ct);
    }
}
