using FluentValidation;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.OpenApi;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Microsoft.AspNetCore.Http;
using Invoria.Ordering.Application.Orders.Commands.CreateOrder;
using Invoria.Ordering.Application.Orders.Commands.UpdateOrder;
using Invoria.Ordering.Contracts.Orders.Dtos;
using Invoria.Ordering.Endpoints.Orders.Requests;
using MediatR;

namespace Invoria.Ordering.Endpoints.Orders;

public class UpdateOrderItemsEndpoint : EndpointBase<UpdateOrderItemsRequest, OrderDto>
{
    private readonly IMediator _mediator;

    public UpdateOrderItemsEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
        : base(resultMapper)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Put("{id}");
        AllowAnonymous();

        Group<OrderRoutingGroup>();

        Summary(s =>
        {
            s.Summary = "Update order line items";
            s.Description = "Replaces order line items for an existing order.";
            s.Responses[StatusCodes.Status200OK] =
                InvoriaOpenApiResponseDescriptions.Ok200 + " Returns the updated order.";
            s.Responses[StatusCodes.Status400BadRequest] = InvoriaOpenApiResponseDescriptions.BadRequest400;
            s.Responses[StatusCodes.Status404NotFound] = InvoriaOpenApiResponseDescriptions.NotFound404;
            s.Responses[StatusCodes.Status409Conflict] = InvoriaOpenApiResponseDescriptions.Conflict409;
            s.Responses[StatusCodes.Status422UnprocessableEntity] = InvoriaOpenApiResponseDescriptions.UnprocessableEntity422;
            s.Responses[StatusCodes.Status500InternalServerError] = InvoriaOpenApiResponseDescriptions.InternalServerError500;
        });
    }

    public override async Task HandleAsync(UpdateOrderItemsRequest req, CancellationToken ct)
    {
        ValidateRequest(req);

        var itemCommands = req.Items
            .Select(i => new CreateOrderItemCommand(i.ProductId, i.Quantity, i.Price))
            .ToList();

        var command = new UpdateOrderCommand(req.Id, itemCommands);

        var result = await _mediator.Send(command, ct);

        await SendResultAsync(result, ct);
    }
}
