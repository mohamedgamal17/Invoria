using Invoria.BuildingBlocks.Domain.Events;
using Invoria.Ordering.Contracts.Events;
using Invoria.Ordering.Contracts.Models;
using Invoria.Ordering.Domain.Orders;
using MediatR;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Invoria.Ordering.Application.Orders.Handlers;

public sealed class OrderEntityUpdatedDomainEventHandler : INotificationHandler<EntityUpdatedDomainEvent<Order, string>>
{
    private readonly IBus _bus;
    private readonly ILogger<OrderEntityUpdatedDomainEventHandler> _logger;

    public OrderEntityUpdatedDomainEventHandler(IBus bus, ILogger<OrderEntityUpdatedDomainEventHandler> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    public async Task Handle(EntityUpdatedDomainEvent<Order, string> notification, CancellationToken cancellationToken)
    {
        var order = notification.Entity;
        var integrationEvent = new OrderUpdatedIntegrationEvent
        {
            OccurredOn = notification.OccurredOn,
            Order = new OrderIntegrationPayload
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                CustomerId = order.CustomerId,
                OrderStatus = order.Status,
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
            nameof(OrderUpdatedIntegrationEvent),
            integrationEvent.Order.Id,
            integrationEvent.Order.OrderNumber);
        await _bus.Publish(integrationEvent);
    }
}
