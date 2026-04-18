using FluentValidation;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.OpenApi;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Microsoft.AspNetCore.Http;
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

        Summary(s =>
        {
            s.Summary = "List batches";
            s.Description = "Returns a paged list of batches, optionally filtered by product and state.";
            s.Responses[StatusCodes.Status200OK] =
                InvoriaOpenApiResponseDescriptions.Ok200 + " Returns paged batch data.";
            s.Responses[StatusCodes.Status400BadRequest] = InvoriaOpenApiResponseDescriptions.BadRequest400;
            s.Responses[StatusCodes.Status500InternalServerError] = InvoriaOpenApiResponseDescriptions.InternalServerError500;
        });
    }

    public override async Task HandleAsync(ListBatchesRequest req, CancellationToken ct)
    {
        ValidateRequest(req);

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
