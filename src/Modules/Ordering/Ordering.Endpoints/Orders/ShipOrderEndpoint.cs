using FluentValidation;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.OpenApi;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Microsoft.AspNetCore.Http;
using Invoria.Ordering.Application.Orders.Commands.ShipOrder;
using Invoria.Ordering.Contracts.Dtos;
using Invoria.Ordering.Endpoints.Orders.Requests;
using MediatR;

namespace Invoria.Ordering.Endpoints.Orders;

public class ShipOrderEndpoint : EndpointBase<ShipOrderRequest, OrderDto>
{
    private readonly IMediator _mediator;

    public ShipOrderEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
        : base(resultMapper)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("{id}/ship");
        AllowAnonymous();

        Group<OrderRoutingGroup>();

        Summary(s =>
        {
            s.Summary = "Mark order as shipped";
            s.Description = "Transitions the order to Shipped after dispatch.";
            s.Responses[StatusCodes.Status200OK] =
                InvoriaOpenApiResponseDescriptions.Ok200 + " Returns the updated order.";
            s.Responses[StatusCodes.Status400BadRequest] = InvoriaOpenApiResponseDescriptions.BadRequest400;
            s.Responses[StatusCodes.Status404NotFound] = InvoriaOpenApiResponseDescriptions.NotFound404;
            s.Responses[StatusCodes.Status409Conflict] = InvoriaOpenApiResponseDescriptions.Conflict409;
            s.Responses[StatusCodes.Status422UnprocessableEntity] = InvoriaOpenApiResponseDescriptions.UnprocessableEntity422;
            s.Responses[StatusCodes.Status500InternalServerError] = InvoriaOpenApiResponseDescriptions.InternalServerError500;
        });
    }

    public override async Task HandleAsync(ShipOrderRequest req, CancellationToken ct)
    {
        ValidateRequest(req);

        var command = new ShipOrderCommand(req.Id);

        var result = await _mediator.Send(command, ct);

        await SendResultAsync(result, ct);
    }
}
