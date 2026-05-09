using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.OpenApi;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Microsoft.AspNetCore.Http;
using Invoria.Procurement.Application.PurchaseOrders.Queries.ListPurchaseOrders;
using Invoria.Procurement.Contracts.Dtos;
using Invoria.Procurement.Endpoints.PurchaseOrders.Requests;
using MediatR;

namespace Invoria.Procurement.Endpoints.PurchaseOrders;

public sealed class ListPurchaseOrdersEndpoint : EndpointBase<ListPurchaseOrdersRequest, PagingDto<PurchaseOrderDto>>
{
    private readonly IMediator _mediator;

    public ListPurchaseOrdersEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
        : base(resultMapper)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("");
        AllowAnonymous();
        Group<PurchaseOrderRoutingGroup>();

        Summary(s =>
        {
            s.Summary = "List purchase orders";
            s.Description = "Returns a paged list of purchase orders with optional includes.";
            s.Responses[StatusCodes.Status200OK] =
                InvoriaOpenApiResponseDescriptions.Ok200 + " Returns paged purchase order data.";
            s.Responses[StatusCodes.Status400BadRequest] = InvoriaOpenApiResponseDescriptions.BadRequest400;
            s.Responses[StatusCodes.Status500InternalServerError] = InvoriaOpenApiResponseDescriptions.InternalServerError500;
        });
    }

    public override async Task HandleAsync(ListPurchaseOrdersRequest req, CancellationToken ct)
    {
        ValidateRequest(req);

        var query = new ListPurchaseOrdersQuery
        {
            Skip = req.Skip,
            Length = req.Length,
            Number = req.Number,
            Status = req.Status,
            IncludePurchaseItems = req.IncludePurchaseItems,
            IncludeSupplier = req.IncludeSupplier
        };

        var result = await _mediator.Send(query, ct);
        await SendResultAsync(result, ct);
    }
}
