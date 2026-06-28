using FluentValidation;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.OpenApi;
using Invoria.BuildingBlocks.Infrastructure.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Invoria.Inventory.Application.Returns.Queries.ListReturns;
using Invoria.Inventory.Contracts.Returns.Dtos;
using Invoria.Inventory.Endpoints.Returns.Requests;

namespace Invoria.Inventory.Endpoints.Returns;

public class ListReturnsEndpoint
    : EndpointBase<ListReturnsRequest, PagingDto<ReturnDto>>
{
    private readonly IMediator _mediator;

    public ListReturnsEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
        : base(resultMapper)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("");
        AllowAnonymous();

        Group<ReturnRoutingGroup>();

        Summary(s =>
        {
            s.Summary = "List returns";
            s.Description = "Returns a paged list of returns, optionally filtered by return type.";
            s.Responses[StatusCodes.Status200OK] =
                InvoriaOpenApiResponseDescriptions.Ok200 + " Returns paged return data.";
            s.Responses[StatusCodes.Status400BadRequest] =
                InvoriaOpenApiResponseDescriptions.BadRequest400;
            s.Responses[StatusCodes.Status500InternalServerError] =
                InvoriaOpenApiResponseDescriptions.InternalServerError500;
        });
    }

    public override async Task HandleAsync(ListReturnsRequest req, CancellationToken ct)
    {
        ValidateRequest(req);

        var query = new ListReturnsQuery
        {
            Skip = req.Skip,
            Length = req.Length,
            Type = req.Type
        };

        var result = await _mediator.Send(query, ct);

        await SendResultAsync(result, ct);
    }
}
