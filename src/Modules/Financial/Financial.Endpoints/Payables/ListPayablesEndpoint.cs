using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.OpenApi;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Invoria.Financial.Application.Payables.Queries.ListPayables;
using Invoria.Financial.Contracts.Payables.Dtos;
using Invoria.Financial.Endpoints.Payables.Requests;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Invoria.Financial.Endpoints.Payables;

public class ListPayablesEndpoint : EndpointBase<ListPayablesRequest, PagingDto<PayableDto>>
{
    private readonly IMediator _mediator;

    public ListPayablesEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
        : base(resultMapper)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("");
        AllowAnonymous();

        Group<PayableRoutingGroup>();

        Summary(s =>
        {
            s.Summary = "List payables";
            s.Description =
                "Returns a paged list of payables with optional filters by party id, source id and paid status.";
            s.Responses[StatusCodes.Status200OK] =
                InvoriaOpenApiResponseDescriptions.Ok200 + " Returns paged payable data.";
            s.Responses[StatusCodes.Status400BadRequest] = InvoriaOpenApiResponseDescriptions.BadRequest400;
            s.Responses[StatusCodes.Status500InternalServerError] =
                InvoriaOpenApiResponseDescriptions.InternalServerError500;
        });
    }

    public override async Task HandleAsync(ListPayablesRequest req, CancellationToken ct)
    {
        ValidateRequest(req);

        var query = new ListPayablesQuery
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
