using Invoria.Inventory.Contracts.Allocations.Events;
using Invoria.Inventory.Contracts.Allocations.Models;
using Invoria.Ordering.Contracts.Orders.Models;

namespace Invoria.Ordering.Application.Orders.Extensions;

public static class OrderAcceptedMappingExtensions
{
    public static AllocateOrderIntegrationEvent ToAllocateOrderIntegrationEvent(this OrderModel order) =>
        new()
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerId = order.CustomerId,
            Items = order.Lines
                .Select(l => new AllocateOrderLineModel
                {
                    Id = l.Id,
                    ProductId = l.ProductId,
                    Quantity = l.Quantity
                })
                .ToList()
        };
}
