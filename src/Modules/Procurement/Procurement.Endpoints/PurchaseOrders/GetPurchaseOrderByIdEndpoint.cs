using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Invoria.Procurement.Application.PurchaseOrders.Queries.GetPurchaseOrderById;
using Invoria.Procurement.Contracts.Dtos;
using Invoria.Procurement.Endpoints.PurchaseOrders.Requests;
using MediatR;

namespace Invoria.Procurement.Endpoints.PurchaseOrders;

public sealed class GetPurchaseOrderByIdEndpoint : EndpointBase<GetPurchaseOrderByIdRequest, PurchaseOrderDto>
{
    private readonly IMediator _mediator;

    public GetPurchaseOrderByIdEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
        : base(resultMapper)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("{id}");
        AllowAnonymous();
        Group<PurchaseOrderRoutingGroup>();
    }

    public override async Task HandleAsync(GetPurchaseOrderByIdRequest req, CancellationToken ct)
    {
        ValidateRequest(req);

        var query = new GetPurchaseOrderByIdQuery
        {
            Id = req.Id
        };

        var result = await _mediator.Send(query, ct);
        await SendResultAsync(result, ct);
    }
}
