using Invoria.Ordering.Contracts.Events;
using Invoria.Reporting.Domain.Orders;
using Invoria.Reporting.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;

namespace Invoria.Reporting.Application.Orders.Consumers;

public sealed class OrderUpdatedIntegrationEventConsumer : IHandleMessages<OrderUpdatedIntegrationEvent>
{
    private readonly IReportedOrderRepository _reportedOrderRepository;
    private readonly ILogger<OrderUpdatedIntegrationEventConsumer> _logger;

    public OrderUpdatedIntegrationEventConsumer(
        IReportedOrderRepository reportedOrderRepository,
        ILogger<OrderUpdatedIntegrationEventConsumer> logger)
    {
        _reportedOrderRepository = reportedOrderRepository;
        _logger = logger;
    }

    public async Task Handle(OrderUpdatedIntegrationEvent message)
    {
        var order = message.Order;
        var occurred = message.OccurredOn;

        _logger.LogDebug(
            "Consuming integration event {EventName} for OrderId={OrderId} OrderNumber={OrderNumber}",
            nameof(OrderUpdatedIntegrationEvent),
            order.Id,
            order.OrderNumber);

        var existing = await _reportedOrderRepository.GetByIdWithGraphAsync(order.Id, CancellationToken.None);

        if (existing is not null)
        {
            if (existing.SourceLastKnownAt is { } lastKnown && occurred <= lastKnown)
            {
                return;
            }

            existing.OrderNumber = order.OrderNumber;
            existing.CustomerId = order.CustomerId;
            existing.OrderStatus = order.OrderStatus;
            existing.PaymentType = order.PaymentType;
            existing.PaymentStatus = order.PaymentStatus;
            existing.TotalOrderAmount = order.TotalOrderAmount;
            existing.AmountPaid = order.AmountPaid;
            existing.AmountOutstanding = order.AmountOutstanding;
            existing.ReplicationVersion = existing.ReplicationVersion + 1;
            existing.SourceLastKnownAt = occurred;
            existing.LastModifiedAt = occurred;

            existing.Lines.Clear();
            foreach (var l in order.Lines)
            {
                existing.Lines.Add(new ReportedOrderLine
                {
                    Id = l.Id,
                    ReportedOrderId = order.Id,
                    ProductId = l.ProductId,
                    Quantity = l.Quantity,
                    UnitPrice = l.UnitPrice,
                    LineTotal = l.LineTotal,
                    ReportedOrder = existing
                });
            }

            await _reportedOrderRepository.UpsertGraphAsync(existing, CancellationToken.None);
            return;
        }

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

        await _reportedOrderRepository.UpsertGraphAsync(reported, CancellationToken.None);
    }
}
