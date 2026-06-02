using Invoria.Ordering.Contracts.Orders.Events;
using Invoria.Ordering.Contracts.Orders.Models;
using Invoria.Ordering.Domain;
using Invoria.Ordering.Domain.Orders;
using Invoria.Ordering.Domain.Orders.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rebus.Bus;

namespace Invoria.Ordering.Application.Orders.Handlers;

public sealed class OrderCreatedDomainEventHandler : INotificationHandler<OrderCreatedDomainEvent>
{
    private readonly IOrderingRepository<Order> _orderRepository;
    private readonly IBus _bus;
    private readonly ILogger<OrderCreatedDomainEventHandler> _logger;

    public OrderCreatedDomainEventHandler(
        IOrderingRepository<Order> orderRepository,
        IBus bus,
        ILogger<OrderCreatedDomainEventHandler> logger)
    {
        _orderRepository = orderRepository;
        _bus = bus;
        _logger = logger;
    }

    public async Task Handle(OrderCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var order = await _orderRepository
            .AsQuerable()
            .Include(o => o.Items)
            .SingleOrDefaultAsync(o => o.Id == notification.OrderId, cancellationToken);

        if (order is null)
        {
            throw new InvalidOperationException($"Order with ID {notification.OrderId} not found.");
        }

        var integrationEvent = new OrderCreatedIntegrationEvent
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
            nameof(OrderCreatedIntegrationEvent),
            integrationEvent.Order.Id,
            integrationEvent.Order.OrderNumber);
        await _bus.Publish(integrationEvent);
    }
}
