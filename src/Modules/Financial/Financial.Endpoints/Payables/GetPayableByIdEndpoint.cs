using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.OpenApi;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Invoria.Financial.Application.Payables.Queries.GetPayableById;
using Invoria.Financial.Contracts.Payables.Dtos;
using Invoria.Financial.Endpoints.Payables.Requests;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Invoria.Financial.Endpoints.Payables;

public class GetPayableByIdEndpoint : EndpointBase<GetPayableByIdRequest, PayableDto>
{
    private readonly IMediator _mediator;

    public GetPayableByIdEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
        : base(resultMapper)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("{id}");
        AllowAnonymous();

        Group<PayableRoutingGroup>();

        Summary(s =>
        {
            s.Summary = "Get payable by id";
            s.Description = "Returns a single payable by identifier, including settlements.";
            s.Responses[StatusCodes.Status200OK] =
                InvoriaOpenApiResponseDescriptions.Ok200 + " Returns the payable DTO.";
            s.Responses[StatusCodes.Status400BadRequest] = InvoriaOpenApiResponseDescriptions.BadRequest400;
            s.Responses[StatusCodes.Status404NotFound] = InvoriaOpenApiResponseDescriptions.NotFound404;
            s.Responses[StatusCodes.Status500InternalServerError] =
                InvoriaOpenApiResponseDescriptions.InternalServerError500;
        });
    }

    public override async Task HandleAsync(GetPayableByIdRequest req, CancellationToken ct)
    {
        ValidateRequest(req);

        var query = new GetPayableByIdQuery
        {
            Id = req.Id
        };

        var result = await _mediator.Send(query, ct);

        await SendResultAsync(result, ct);
    }
}
