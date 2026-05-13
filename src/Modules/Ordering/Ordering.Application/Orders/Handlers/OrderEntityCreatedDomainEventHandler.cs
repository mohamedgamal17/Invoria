using Invoria.BuildingBlocks.Domain.Events;
using Invoria.Ordering.Contracts.Events;
using Invoria.Ordering.Contracts.Models;
using Invoria.Ordering.Domain.Orders;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Invoria.Ordering.Application.Orders.Handlers;

public sealed class OrderEntityCreatedDomainEventHandler : INotificationHandler<EntityCreatedDomainEvent<Order, string>>
{
    private readonly IBus _bus;
    private readonly ILogger<OrderEntityCreatedDomainEventHandler> _logger;

    public OrderEntityCreatedDomainEventHandler(IBus bus, ILogger<OrderEntityCreatedDomainEventHandler> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    public async Task Handle(EntityCreatedDomainEvent<Order, string> notification, CancellationToken cancellationToken)
    {
        var order = notification.Entity;
        var integrationEvent = new OrderCreatedIntegrationEvent
        {
            OccurredOn = notification.OccurredOn,
            Order = new OrderIntegrationPayload
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                CustomerId = order.CustomerId,
                OrderStatus = order.Status,
                FullfillmentStatus = order.FullfillmentStatus,
                PaymentType = order.PaymentType,
                PaymentStatus = order.PaymentStatus,
                TotalOrderAmount = order.TotalOrderAmount,
                AmountPaid = order.AmountPaid,
                AmountOutstanding = order.AmountOutstanding,
                Lines = order.Items
                    .Select(i => new OrderLineModel
                    {
                        Id = i.Id,
                        ProductId = i.ProductId,
                        Quantity = i.Quantity,
                        UnitPrice = i.Price,
                        LineTotal = i.Price * i.Quantity
                    })
                    .ToList()
            }
        };

        _logger.LogDebug(
            "Publishing integration event {EventName} for OrderId={OrderId} OrderNumber={OrderNumber}",
            nameof(OrderCreatedIntegrationEvent),
            integrationEvent.Order.Id,
            integrationEvent.Order.OrderNumber);
        await _bus.Publish(integrationEvent);
    }
}
