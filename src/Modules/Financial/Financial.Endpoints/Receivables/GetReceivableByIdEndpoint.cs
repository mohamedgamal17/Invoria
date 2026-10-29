using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.OpenApi;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Invoria.Financial.Application.Receivables.Queries.GetReceivableById;
using Invoria.Financial.Contracts.Receivables.Dtos;
using Invoria.Financial.Endpoints.Receivables.Requests;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Invoria.Financial.Endpoints.Receivables;

public class GetReceivableByIdEndpoint : EndpointBase<GetReceivableByIdRequest, ReceivableDto>
{
    private readonly IMediator _mediator;

    public GetReceivableByIdEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
        : base(resultMapper)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("{id}");
        AllowAnonymous();

        Group<ReceivableRoutingGroup>();

        Summary(s =>
        {
            s.Summary = "Get receivable by id";
            s.Description = "Returns a single receivable by identifier, including settlements.";
            s.Responses[StatusCodes.Status200OK] =
                InvoriaOpenApiResponseDescriptions.Ok200 + " Returns the receivable DTO.";
            s.Responses[StatusCodes.Status400BadRequest] = InvoriaOpenApiResponseDescriptions.BadRequest400;
            s.Responses[StatusCodes.Status404NotFound] = InvoriaOpenApiResponseDescriptions.NotFound404;
            s.Responses[StatusCodes.Status500InternalServerError] =
                InvoriaOpenApiResponseDescriptions.InternalServerError500;
        });
    }

    public override async Task HandleAsync(GetReceivableByIdRequest req, CancellationToken ct)
    {
        ValidateRequest(req);

        var query = new GetReceivableByIdQuery
        {
            Id = req.Id
        };

        var result = await _mediator.Send(query, ct);

        await SendResultAsync(result, ct);
    }
}
