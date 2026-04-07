using Invoria.Ordering.Contracts.Events;
using Invoria.Ordering.Domain.Orders.Events;
using MediatR;
using Rebus.Bus;

namespace Invoria.Ordering.Application.Orders.Handlers;

public sealed class OrderRefusedDomainEventHandler : INotificationHandler<OrderRefusedDomainEvent>
{
    private readonly IBus _bus;

    public OrderRefusedDomainEventHandler(IBus bus)
    {
        _bus = bus;
    }

    public async Task Handle(OrderRefusedDomainEvent notification, CancellationToken cancellationToken)
    {
        await _bus.Publish(
            new OrderRefusedIntegrationEvent
            {
                OrderId = notification.OrderId,
                OrderNumber = notification.OrderNumber,
                CustomerId = notification.CustomerId,
                RefusedAt = notification.OccurredOn
            });
    }
}
