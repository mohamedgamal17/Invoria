using FluentValidation;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.OpenApi;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Invoria.Ordering.Application.Orders.Commands.AddReturnItems;
using Invoria.Ordering.Contracts.Dtos;
using Invoria.Ordering.Endpoints.Orders.Requests;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Invoria.Ordering.Endpoints.Orders;

public class AddReturnItemsEndpoint : EndpointBase<AddReturnItemsRequest, OrderDto>
{
    private readonly IMediator _mediator;

    public AddReturnItemsEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
        : base(resultMapper)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Put("{id}/return-items");
        AllowAnonymous();

        Group<OrderRoutingGroup>();

        Summary(s =>
        {
            s.Summary = "Record order return items";
            s.Description = "Replaces the full return list for a shipped order.";
            s.Responses[StatusCodes.Status200OK] =
                InvoriaOpenApiResponseDescriptions.Ok200 + " Returns the updated order.";
            s.Responses[StatusCodes.Status400BadRequest] = InvoriaOpenApiResponseDescriptions.BadRequest400;
            s.Responses[StatusCodes.Status404NotFound] = InvoriaOpenApiResponseDescriptions.NotFound404;
            s.Responses[StatusCodes.Status409Conflict] = InvoriaOpenApiResponseDescriptions.Conflict409;
            s.Responses[StatusCodes.Status422UnprocessableEntity] = InvoriaOpenApiResponseDescriptions.UnprocessableEntity422;
            s.Responses[StatusCodes.Status500InternalServerError] = InvoriaOpenApiResponseDescriptions.InternalServerError500;
        });
    }

    public override async Task HandleAsync(AddReturnItemsRequest req, CancellationToken ct)
    {
        ValidateRequest(req);

        var lines = (req.Items ?? [])
            .Select(i => new AddReturnItemLine(i.OrderItemId, i.Quantity))
            .ToList();

        var command = new AddReturnItemsCommand(req.Id, lines);

        var result = await _mediator.Send(command, ct);

        await SendResultAsync(result, ct);
    }
}
