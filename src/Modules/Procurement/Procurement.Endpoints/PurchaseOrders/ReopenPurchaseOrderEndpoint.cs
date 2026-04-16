using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Invoria.Procurement.Application.PurchaseOrders.Commands.ReopenPurchaseOrder;
using Invoria.Procurement.Contracts.Dtos;
using Invoria.Procurement.Endpoints.PurchaseOrders.Requests;
using MediatR;

namespace Invoria.Procurement.Endpoints.PurchaseOrders;

public sealed class ReopenPurchaseOrderEndpoint : EndpointBase<ReopenPurchaseOrderRequest, PurchaseOrderDto>
{
    private readonly IMediator _mediator;

    public ReopenPurchaseOrderEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
        : base(resultMapper)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("{id}/reopen");
        AllowAnonymous();
        Group<PurchaseOrderRoutingGroup>();
    }

    public override async Task HandleAsync(ReopenPurchaseOrderRequest req, CancellationToken ct)
    {
        ValidateRequest(req);

        var command = new ReopenPurchaseOrderCommand(req.Id);
        var result = await _mediator.Send(command, ct);

        await SendResultAsync(result, ct);
    }
}
