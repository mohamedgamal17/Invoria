using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Invoria.Procurement.Application.PurchaseOrders.Commands.SubmitPurchaseOrder;
using Invoria.Procurement.Contracts.Dtos;
using Invoria.Procurement.Endpoints.PurchaseOrders.Requests;
using MediatR;

namespace Invoria.Procurement.Endpoints.PurchaseOrders;

public sealed class SubmitPurchaseOrderEndpoint : EndpointBase<SubmitPurchaseOrderRequest, PurchaseOrderDto>
{
    private readonly IMediator _mediator;

    public SubmitPurchaseOrderEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
        : base(resultMapper)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("{id}/submit");
        AllowAnonymous();
        Group<PurchaseOrderRoutingGroup>();
    }

    public override async Task HandleAsync(SubmitPurchaseOrderRequest req, CancellationToken ct)
    {
        ValidateRequest(req);

        var command = new SubmitPurchaseOrderCommand(req.Id);
        var result = await _mediator.Send(command, ct);

        await SendResultAsync(result, ct);
    }
}
