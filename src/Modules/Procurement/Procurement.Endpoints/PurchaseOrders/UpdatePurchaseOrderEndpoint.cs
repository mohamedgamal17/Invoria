using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.OpenApi;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Invoria.Procurement.Application.PurchaseOrders.Commands.UpdatePurchaseOrder;
using Invoria.Procurement.Contracts.Dtos;
using Invoria.Procurement.Endpoints.PurchaseOrders.Requests;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Invoria.Procurement.Endpoints.PurchaseOrders;

public sealed class UpdatePurchaseOrderEndpoint : EndpointBase<UpdatePurchaseOrderRequest, PurchaseOrderDto>
{
    private readonly IMediator _mediator;

    public UpdatePurchaseOrderEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
        : base(resultMapper)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Put("{id}");
        AllowAnonymous();
        Group<PurchaseOrderRoutingGroup>();

        Summary(s =>
        {
            s.Summary = "Update purchase order";
            s.Description = "Updates purchase order header and line items when allowed (Draft/Reopened).";
            s.Responses[StatusCodes.Status200OK] =
                InvoriaOpenApiResponseDescriptions.Ok200 + " Returns the updated purchase order.";
            s.Responses[StatusCodes.Status400BadRequest] = InvoriaOpenApiResponseDescriptions.BadRequest400;
            s.Responses[StatusCodes.Status404NotFound] = InvoriaOpenApiResponseDescriptions.NotFound404;
            s.Responses[StatusCodes.Status409Conflict] = InvoriaOpenApiResponseDescriptions.Conflict409;
            s.Responses[StatusCodes.Status422UnprocessableEntity] = InvoriaOpenApiResponseDescriptions.UnprocessableEntity422;
            s.Responses[StatusCodes.Status500InternalServerError] = InvoriaOpenApiResponseDescriptions.InternalServerError500;
        });
    }

    public override async Task HandleAsync(UpdatePurchaseOrderRequest req, CancellationToken ct)
    {
        ValidateRequest(req);

        var itemCommands = req.PurchaseOrderItems
            .Select(x => new UpdatePurchaseOrderItemCommand(x.ProductId, x.Quantity, x.UnitPrice, x.SupplierProductCode))
            .ToList();

        var command = new UpdatePurchaseOrderCommand(
            id: req.Id,
            supplierId: req.SupplierId,
            taxAmount: req.TaxAmount,
            discountAmount: req.DiscountAmount,
            purchaseOrderItems: itemCommands);

        var result = await _mediator.Send(command, ct);
        await SendResultAsync(result, ct);
    }
}

