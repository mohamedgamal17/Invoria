using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.OpenApi;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Microsoft.AspNetCore.Http;
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

        Summary(s =>
        {
            s.Summary = "Create purchase order";
            s.Description = "Creates a new purchase order for a supplier with line items and amounts.";
            s.Responses[StatusCodes.Status200OK] =
                InvoriaOpenApiResponseDescriptions.Ok200 + " Returns the created purchase order.";
            s.Responses[StatusCodes.Status400BadRequest] = InvoriaOpenApiResponseDescriptions.BadRequest400;
            s.Responses[StatusCodes.Status422UnprocessableEntity] = InvoriaOpenApiResponseDescriptions.UnprocessableEntity422;
            s.Responses[StatusCodes.Status500InternalServerError] = InvoriaOpenApiResponseDescriptions.InternalServerError500;
        });
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
