using Invoria.Ordering.Contracts.Events;
using Invoria.Ordering.Contracts.Models;
using Invoria.Ordering.Domain.Orders.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Invoria.Ordering.Application.Orders.Handlers;

public sealed class OrderRefusalReleaseRequestedDomainEventHandler
    : INotificationHandler<OrderRefusalReleaseRequestedDomainEvent>
{
    private readonly IBus _bus;
    private readonly ILogger<OrderRefusalReleaseRequestedDomainEventHandler> _logger;

    public OrderRefusalReleaseRequestedDomainEventHandler(
        IBus bus,
        ILogger<OrderRefusalReleaseRequestedDomainEventHandler> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    public async Task Handle(OrderRefusalReleaseRequestedDomainEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new ReleaseOrderAllocationsIntegrationEvent
        {
            Id = notification.OrderId,
            OrderNumber = notification.OrderNumber,
            CustomerId = notification.CustomerId,
            ReleaseReason = AllocationReleaseReason.Refusal,
            Items = notification.Lines
                .Select(l => new OrderItemModel
                {
                    Id = l.OrderItemId,
                    ProductId = l.ProductId,
                    Quantity = l.Quantity
                })
                .ToList()
        };

        _logger.LogDebug(
            "Publishing integration event {EventName} for OrderId={OrderId} OrderNumber={OrderNumber}",
            nameof(ReleaseOrderAllocationsIntegrationEvent),
            integrationEvent.Id,
            integrationEvent.OrderNumber);
        await _bus.Publish(integrationEvent);
    }
}
