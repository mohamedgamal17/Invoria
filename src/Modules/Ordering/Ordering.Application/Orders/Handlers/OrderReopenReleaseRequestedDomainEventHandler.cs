using Invoria.Ordering.Contracts.Events;
using Invoria.Ordering.Contracts.Models;
using Invoria.Ordering.Domain.Orders.Events;
using MediatR;
using Rebus.Bus;

namespace Invoria.Ordering.Application.Orders.Handlers;

public sealed class OrderReopenReleaseRequestedDomainEventHandler
    : INotificationHandler<OrderReopenReleaseRequestedDomainEvent>
{
    private readonly IBus _bus;

    public OrderReopenReleaseRequestedDomainEventHandler(IBus bus)
    {
        _bus = bus;
    }

    public async Task Handle(OrderReopenReleaseRequestedDomainEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new ReleaseOrderAllocationsIntegrationEvent
        {
            Id = notification.OrderId,
            OrderNumber = notification.OrderNumber,
            CustomerId = notification.CustomerId,
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
