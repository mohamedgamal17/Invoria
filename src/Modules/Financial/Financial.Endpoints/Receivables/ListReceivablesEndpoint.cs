using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.OpenApi;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Invoria.Financial.Application.Receivables.Queries.ListReceivables;
using Invoria.Financial.Contracts.Receivables.Dtos;
using Invoria.Financial.Endpoints.Receivables.Requests;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Invoria.Financial.Endpoints.Receivables;

public class ListReceivablesEndpoint : EndpointBase<ListReceivablesRequest, PagingDto<ReceivableDto>>
{
    private readonly IMediator _mediator;

    public ListReceivablesEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
        : base(resultMapper)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("");
        AllowAnonymous();

        Group<ReceivableRoutingGroup>();

        Summary(s =>
        {
            s.Summary = "List receivables";
            s.Description =
                "Returns a paged list of receivables with optional filters by party id, source id and paid status.";
            s.Responses[StatusCodes.Status200OK] =
                InvoriaOpenApiResponseDescriptions.Ok200 + " Returns paged receivable data.";
            s.Responses[StatusCodes.Status400BadRequest] = InvoriaOpenApiResponseDescriptions.BadRequest400;
            s.Responses[StatusCodes.Status500InternalServerError] =
                InvoriaOpenApiResponseDescriptions.InternalServerError500;
        });
    }

    public override async Task HandleAsync(ListReceivablesRequest req, CancellationToken ct)
    {
        ValidateRequest(req);

        var query = new ListReceivablesQuery
        {
            Skip = req.Skip,
            Length = req.Length,
            PartyId = req.PartyId,
            SourceId = req.SourceId,
            IsPaid = req.IsPaid
        };

        var result = await _mediator.Send(query, ct);

        await SendResultAsync(result, ct);
    }
}
