using FluentValidation;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Invoria.BuildingBlocks.Infrastructure.Results;
using Invoria.Ordering.Application.Orders.Commands.AcceptOrder;
using Invoria.Ordering.Contracts.Dtos;
using Invoria.Ordering.Endpoints.Orders.Requests;
using MediatR;

namespace Invoria.Ordering.Endpoints.Orders;

public class AcceptOrderEndpoint : EndpointBase<AcceptOrderRequest, OrderDto>
{
    private readonly IMediator _mediator;

    public AcceptOrderEndpoint(IResultToHttpMapper resultMapper, IMediator mediator)
        : base(resultMapper)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("{id}/accept");
        AllowAnonymous();

        Group<OrderRoutingGroup>();
        
    }

    public override async Task HandleAsync(AcceptOrderRequest req, CancellationToken ct)
    {
        ValidateRequest(req);

        var command = new AcceptOrderCommand(req.Id);

        var result = await _mediator.Send(command, ct);

        await SendResultAsync(result, ct);
    }
}
