using FluentValidation;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.OpenApi;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Microsoft.AspNetCore.Http;
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

        Summary(s =>
        {
            s.Summary = "Create batch";
            s.Description = "Creates a new inventory batch for a product.";
            s.Responses[StatusCodes.Status200OK] =
                InvoriaOpenApiResponseDescriptions.Ok200 + " Returns the created batch.";
            s.Responses[StatusCodes.Status400BadRequest] = InvoriaOpenApiResponseDescriptions.BadRequest400;
            s.Responses[StatusCodes.Status422UnprocessableEntity] = InvoriaOpenApiResponseDescriptions.UnprocessableEntity422;
            s.Responses[StatusCodes.Status500InternalServerError] = InvoriaOpenApiResponseDescriptions.InternalServerError500;
        });
    }

    public override async Task HandleAsync(CreateBatchRequest req, CancellationToken ct)
    {
        ValidateRequest(req);

        var command = new CreateBatchCommand(req.ProductId, req.Quantity, req.PurchasePrice);
        var result = await _mediator.Send(command, ct);

        await SendResultAsync(result, ct);
    }
}
