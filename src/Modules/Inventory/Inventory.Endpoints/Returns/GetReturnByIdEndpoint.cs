using FluentValidation;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.OpenApi;
using Invoria.BuildingBlocks.Infrastructure.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Invoria.Inventory.Application.Returns.Queries.GetReturnById;
using Invoria.Inventory.Contracts.Returns.Dtos;
using Invoria.Inventory.Endpoints.Returns.Requests;

namespace Invoria.Inventory.Endpoints.Returns;

public class GetReturnByIdEndpoint
    : EndpointBase<GetReturnByIdRequest, ReturnDto>
{
    private readonly IMediator _mediator;

    public GetReturnByIdEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
        : base(resultMapper)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("{id}");
        AllowAnonymous();

        Group<ReturnRoutingGroup>();

        Summary(s =>
        {
            s.Summary = "Get return by id";
            s.Description = "Returns a single return by identifier.";
            s.Responses[StatusCodes.Status200OK] =
                InvoriaOpenApiResponseDescriptions.Ok200 + " Returns the return DTO.";
            s.Responses[StatusCodes.Status400BadRequest] =
                InvoriaOpenApiResponseDescriptions.BadRequest400;
            s.Responses[StatusCodes.Status404NotFound] =
                InvoriaOpenApiResponseDescriptions.NotFound404;
            s.Responses[StatusCodes.Status500InternalServerError] =
                InvoriaOpenApiResponseDescriptions.InternalServerError500;
        });
    }

    public override async Task HandleAsync(GetReturnByIdRequest req, CancellationToken ct)
    {
        ValidateRequest(req);

        var query = new GetReturnByIdQuery
        {
            Id = req.Id
        };

        var result = await _mediator.Send(query, ct);

        await SendResultAsync(result, ct);
    }
}
