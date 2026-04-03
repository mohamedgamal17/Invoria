using Invoria.Inventory.Application.Batches.Commands.DispatchOrder;
using Invoria.Ordering.Contracts.Events;
using MediatR;
using Rebus.Handlers;

namespace Invoria.Inventory.Application.Batches.Consumers;

public sealed class OrderDispatchedIntegrationEventConsumer : IHandleMessages<OrderDispatchedIntegrationEvent>
{
    private readonly IMediator _mediator;

    public OrderDispatchedIntegrationEventConsumer(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Handle(OrderDispatchedIntegrationEvent message)
    {
        var result = await _mediator.Send(DispatchOrderCommand.FromEvent(message), CancellationToken.None);
        if (result.IsFailure)
        {
            throw result.Exception!;
        }
    }
}
