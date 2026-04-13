using FluentValidation;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Invoria.Ordering.Application.Orders.Commands.ReopenOrder;
using Invoria.Ordering.Contracts.Dtos;
using Invoria.Ordering.Endpoints.Orders.Requests;
using MediatR;

namespace Invoria.Ordering.Endpoints.Orders;

public class ReopenOrderEndpoint : EndpointBase<ReopenOrderRequest, OrderDto>
{
    private readonly IMediator _mediator;

    public ReopenOrderEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
        : base(resultMapper)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("{id}/reopen");
        AllowAnonymous();

        Group<OrderRoutingGroup>();
    }

    public override async Task HandleAsync(ReopenOrderRequest req, CancellationToken ct)
    {
        ValidateRequest(req);

        var command = new ReopenOrderCommand(req.Id);

        var result = await _mediator.Send(command, ct);

        await SendResultAsync(result, ct);
    }
}
