using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.Results;
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
    }

    public override async Task HandleAsync(ListPurchaseOrdersRequest req, CancellationToken ct)
    {
        ValidateRequest(req);

        var query = new ListPurchaseOrdersQuery
        {
            Skip = req.Skip,
            Length = req.Length,
            Number = req.Number,
            IncludePurchaseItems = req.IncludePurchaseItems,
            IncludeSupplier = req.IncludeSupplier
        };

        var result = await _mediator.Send(query, ct);
        await SendResultAsync(result, ct);
    }
}
