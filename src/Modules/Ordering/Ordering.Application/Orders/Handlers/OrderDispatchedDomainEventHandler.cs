using Invoria.Ordering.Contracts.Events;
using Invoria.Ordering.Contracts.Models;
using Invoria.Ordering.Domain.Orders.Events;
using MediatR;
using Rebus.Bus;

namespace Invoria.Ordering.Application.Orders.Handlers;

public sealed class OrderDispatchedDomainEventHandler : INotificationHandler<OrderDispatchedDomainEvent>
{
    private readonly IBus _bus;

    public OrderDispatchedDomainEventHandler(IBus bus)
    {
        _bus = bus;
    }

    public async Task Handle(OrderDispatchedDomainEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new OrderDispatchedIntegrationEvent
        {
            Id = notification.OrderId,
            OrderNumber = notification.OrderNumber,
            CustomerId = notification.CustomerId,
            DispatchedAt = notification.OccurredOn,
            Items = notification.Lines
                .Select(l => new OrderItemModel
                {
                    Id = l.OrderItemId,
                    ProductId = l.ProductId,
                    Quantity = l.Quantity
                })
                .ToList()
        };

        await _bus.Publish(integrationEvent);
    }
}
