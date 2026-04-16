using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Invoria.Procurement.Application.PurchaseOrders.Commands.CompletePurchaseOrder;
using Invoria.Procurement.Contracts.Dtos;
using Invoria.Procurement.Endpoints.PurchaseOrders.Requests;
using MediatR;

namespace Invoria.Procurement.Endpoints.PurchaseOrders;

public sealed class CompletePurchaseOrderEndpoint : EndpointBase<CompletePurchaseOrderRequest, PurchaseOrderDto>
{
    private readonly IMediator _mediator;

    public CompletePurchaseOrderEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
        : base(resultMapper)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("{id}/complete");
        AllowAnonymous();
        Group<PurchaseOrderRoutingGroup>();
    }

    public override async Task HandleAsync(CompletePurchaseOrderRequest req, CancellationToken ct)
    {
        ValidateRequest(req);

        var command = new CompletePurchaseOrderCommand(req.Id);
        var result = await _mediator.Send(command, ct);

        await SendResultAsync(result, ct);
    }
}

