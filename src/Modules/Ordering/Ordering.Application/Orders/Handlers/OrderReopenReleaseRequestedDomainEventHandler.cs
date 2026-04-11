using Invoria.Ordering.Contracts.Events;
using Invoria.Ordering.Contracts.Models;
using Invoria.Ordering.Domain.Orders.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Invoria.Ordering.Application.Orders.Handlers;

public sealed class OrderReopenReleaseRequestedDomainEventHandler
    : INotificationHandler<OrderReopenReleaseRequestedDomainEvent>
{
    private readonly IBus _bus;
    private readonly ILogger<OrderReopenReleaseRequestedDomainEventHandler> _logger;

    public OrderReopenReleaseRequestedDomainEventHandler(
        IBus bus,
        ILogger<OrderReopenReleaseRequestedDomainEventHandler> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    public async Task Handle(OrderReopenReleaseRequestedDomainEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new ReleaseOrderAllocationsIntegrationEvent
        {
            Id = notification.OrderId,
            OrderNumber = notification.OrderNumber,
            CustomerId = notification.CustomerId,
            ReleaseReason = AllocationReleaseReason.Reopen,
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
