using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Invoria.Procurement.Application.PurchaseOrders.Commands.RejectPurchaseOrder;
using Invoria.Procurement.Contracts.Dtos;
using Invoria.Procurement.Endpoints.PurchaseOrders.Requests;
using MediatR;

namespace Invoria.Procurement.Endpoints.PurchaseOrders;

public sealed class RejectPurchaseOrderEndpoint : EndpointBase<RejectPurchaseOrderRequest, PurchaseOrderDto>
{
    private readonly IMediator _mediator;

    public RejectPurchaseOrderEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
        : base(resultMapper)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("{id}/reject");
        AllowAnonymous();
        Group<PurchaseOrderRoutingGroup>();
    }

    public override async Task HandleAsync(RejectPurchaseOrderRequest req, CancellationToken ct)
    {
        ValidateRequest(req);

        var command = new RejectPurchaseOrderCommand(req.Id);
        var result = await _mediator.Send(command, ct);

        await SendResultAsync(result, ct);
    }
}
