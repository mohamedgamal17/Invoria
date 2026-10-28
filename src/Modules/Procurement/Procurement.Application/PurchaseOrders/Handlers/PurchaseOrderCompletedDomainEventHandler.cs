using Invoria.Financial.Contracts.Payables.Events;
using Invoria.Procurement.Contracts.Events;
using Invoria.Procurement.Contracts.Models;
using Invoria.Procurement.Domain.PurchaseOrders.Events;
using MediatR;
using Rebus.Bus;

namespace Invoria.Procurement.Application.PurchaseOrders.Handlers;

public sealed class PurchaseOrderCompletedDomainEventHandler : INotificationHandler<PurchaseOrderCompletedDomainEvent>
{
    private readonly IBus _bus;

    public PurchaseOrderCompletedDomainEventHandler(IBus bus)
    {
        _bus = bus;
    }

    public async Task Handle(PurchaseOrderCompletedDomainEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new PurchaseOrderCompletedIntegrationEvent
        {
            PurchaseOrderId = notification.PurchaseOrderId,
            PurchaseNumber = notification.PurchaseNumber,
            SupplierId = notification.SupplierId,
            CompletedAt = notification.CompletedAt,
            Items = notification.Items
                .Select(i => new PurchaseOrderItemModel
                {
                    PurchaseOrderItemId = i.PurchaseOrderItemId,
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                })
                .ToList()
        };

        await _bus.Publish(integrationEvent);

        if (notification.TotalAmount > 0m)
        {
            var payableIntegrationEvent = new CreatePurchaseOrderPayableIntegrationEvent
            {
                PurchaseOrderId = notification.PurchaseOrderId,
                SupplierId = notification.SupplierId,
                Amount = notification.TotalAmount
            };

            await _bus.Publish(payableIntegrationEvent);
        }
    }
}

