using Invoria.Ordering.Contracts.Orders.Events;
using Invoria.Reporting.Domain.Orders;
using Invoria.Reporting.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;

namespace Invoria.Reporting.Application.Orders.Consumers;

public sealed class OrderCreatedIntegrationEventConsumer : IHandleMessages<OrderCreatedIntegrationEvent>
{
    private readonly IReportingRepository<ReportedOrder> _orderRepository;
    private readonly ILogger<OrderCreatedIntegrationEventConsumer> _logger;

    public OrderCreatedIntegrationEventConsumer(
        IReportingRepository<ReportedOrder> orderRepository,
        ILogger<OrderCreatedIntegrationEventConsumer> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task Handle(OrderCreatedIntegrationEvent message)
    {
        var order = message.Order;

        _logger.LogDebug(
            "Consuming integration event {EventName} for OrderId={OrderId} OrderNumber={OrderNumber}",
            nameof(OrderCreatedIntegrationEvent),
            order.Id,
            order.OrderNumber);

        var existing = await _orderRepository.SingleOrDefault(
            o => o.Id == order.Id,
            CancellationToken.None);

        if (existing is not null)
        {
            return;
        }

        var occurred = message.OccurredOn;
        var reported = new ReportedOrder
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerId = order.CustomerId,
            OrderStatus = order.OrderStatus,
            PaymentType = order.PaymentType,
            PaymentStatus = order.PaymentStatus,
            TotalOrderAmount = order.TotalOrderAmount,
            AmountPaid = order.AmountPaid,
            AmountOutstanding = order.AmountOutstanding,
            ReplicationVersion = 1,
            SourceLastKnownAt = occurred,
            CreatedAt = occurred,
            CreatedBy = null,
            LastModifiedAt = null,
            LastModifiedBy = null,
            Lines = order.Lines
                .Select(l => new ReportedOrderLine
                {
                    Id = l.Id,
                    ReportedOrderId = order.Id,
                    ProductId = l.ProductId,
                    Quantity = l.Quantity,
                    UnitPrice = l.UnitPrice,
                    LineTotal = l.LineTotal,
                    ReportedOrder = null
                })
                .ToList()
        };

        await _orderRepository.Add(reported, CancellationToken.None);
    }
}
