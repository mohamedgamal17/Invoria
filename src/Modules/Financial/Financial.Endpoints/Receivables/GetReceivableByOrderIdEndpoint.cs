using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.OpenApi;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Invoria.Financial.Application.Receivables.Queries.GetReceivableByOrderId;
using Invoria.Financial.Contracts.Receivables.Dtos;
using Invoria.Financial.Endpoints.Receivables.Requests;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Invoria.Financial.Endpoints.Receivables;

public class GetReceivableByOrderIdEndpoint : EndpointBase<GetReceivableByOrderIdRequest, ReceivableDto>
{
    private readonly IMediator _mediator;

    public GetReceivableByOrderIdEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
        : base(resultMapper)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("by-order/{orderId}");
        AllowAnonymous();

        Group<ReceivableRoutingGroup>();

        Summary(s =>
        {
            s.Summary = "Get receivable by order id";
            s.Description = "Returns the latest receivable for an order identifier, including settlements.";
            s.Responses[StatusCodes.Status200OK] =
                InvoriaOpenApiResponseDescriptions.Ok200 + " Returns the receivable DTO.";
            s.Responses[StatusCodes.Status400BadRequest] = InvoriaOpenApiResponseDescriptions.BadRequest400;
            s.Responses[StatusCodes.Status404NotFound] = InvoriaOpenApiResponseDescriptions.NotFound404;
            s.Responses[StatusCodes.Status500InternalServerError] =
                InvoriaOpenApiResponseDescriptions.InternalServerError500;
        });
    }

    public override async Task HandleAsync(GetReceivableByOrderIdRequest req, CancellationToken ct)
    {
        ValidateRequest(req);

        var query = new GetReceivableByOrderIdQuery
        {
            OrderId = req.OrderId
        };

        var result = await _mediator.Send(query, ct);

        await SendResultAsync(result, ct);
    }
}
