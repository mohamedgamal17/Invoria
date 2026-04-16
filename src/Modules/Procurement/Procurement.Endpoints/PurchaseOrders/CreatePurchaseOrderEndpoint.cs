using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Invoria.Procurement.Application.PurchaseOrders.Commands.CreatePurchaseOrder;
using Invoria.Procurement.Contracts.Dtos;
using Invoria.Procurement.Endpoints.PurchaseOrders.Requests;
using MediatR;

namespace Invoria.Procurement.Endpoints.PurchaseOrders;

public sealed class CreatePurchaseOrderEndpoint : EndpointBase<CreatePurchaseOrderRequest, PurchaseOrderDto>
{
    private readonly IMediator _mediator;

    public CreatePurchaseOrderEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
        : base(resultMapper)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("");
        AllowAnonymous();
        Group<PurchaseOrderRoutingGroup>();
    }

    public override async Task HandleAsync(CreatePurchaseOrderRequest req, CancellationToken ct)
    {
        ValidateRequest(req);

        var itemCommands = req.PurchaseOrderItems
            .Select(x => new CreatePurchaseOrderItemCommand(x.ProductId, x.Quantity, x.UnitPrice, x.SupplierProductCode))
            .ToList();

        var command = new CreatePurchaseOrderCommand(
            supplierId: req.SupplierId,
            taxAmount: req.TaxAmount,
            discountAmount: req.DiscountAmount,
            orderDate: req.OrderDate,
            expectedDeliveryDate: req.ExpectedDeliveryDate,
            purchaseOrderItems: itemCommands);

        var result = await _mediator.Send(command, ct);
        await SendResultAsync(result, ct);
    }
}
