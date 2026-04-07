using Invoria.Ordering.Application.Orders.Commands.CompleteRefusalAfterInventoryReleased;
using Invoria.Ordering.Contracts.Events;
using MediatR;
using Rebus.Handlers;

namespace Invoria.Ordering.Application.Orders.Consumers;

public sealed class OrderRefusalInventoryReleasedIntegrationEventConsumer
    : IHandleMessages<OrderRefusalInventoryReleasedIntegrationEvent>
{
    private readonly IMediator _mediator;

    public OrderRefusalInventoryReleasedIntegrationEventConsumer(IMediator mediator)
    {
        _mediator = mediator;
    }

    public Task Handle(OrderRefusalInventoryReleasedIntegrationEvent message) =>
        _mediator.Send(
            CompleteRefusalAfterInventoryReleasedCommand.FromEvent(message),
            CancellationToken.None);
}
