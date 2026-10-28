using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.OpenApi;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Invoria.Financial.Application.Payables.Queries.GetPayableByPurchaseOrderId;
using Invoria.Financial.Contracts.Payables.Dtos;
using Invoria.Financial.Endpoints.Payables.Requests;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Invoria.Financial.Endpoints.Payables;

public class GetPayableByPurchaseOrderIdEndpoint : EndpointBase<GetPayableByPurchaseOrderIdRequest, PayableDto>
{
    private readonly IMediator _mediator;

    public GetPayableByPurchaseOrderIdEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
        : base(resultMapper)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("by-purchase-order/{purchaseOrderId}");
        AllowAnonymous();

        Group<PayableRoutingGroup>();

        Summary(s =>
        {
            s.Summary = "Get payable by purchase order id";
            s.Description = "Returns the payable for a purchase order identifier, including settlements.";
            s.Responses[StatusCodes.Status200OK] =
                InvoriaOpenApiResponseDescriptions.Ok200 + " Returns the payable DTO.";
            s.Responses[StatusCodes.Status400BadRequest] = InvoriaOpenApiResponseDescriptions.BadRequest400;
            s.Responses[StatusCodes.Status404NotFound] = InvoriaOpenApiResponseDescriptions.NotFound404;
            s.Responses[StatusCodes.Status500InternalServerError] =
                InvoriaOpenApiResponseDescriptions.InternalServerError500;
        });
    }

    public override async Task HandleAsync(GetPayableByPurchaseOrderIdRequest req, CancellationToken ct)
    {
        ValidateRequest(req);

        var query = new GetPayableByPurchaseOrderIdQuery
        {
            PurchaseOrderId = req.PurchaseOrderId
        };

        var result = await _mediator.Send(query, ct);

        await SendResultAsync(result, ct);
    }
}
