using Invoria.Inventory.Contracts.Allocations.Events;
using Invoria.Inventory.Contracts.Allocations.Models;
using Invoria.Inventory.Contracts.Returns.Events;
using Invoria.Inventory.Contracts.Returns.Models;
using Invoria.Ordering.Contracts.Orders.Events;
using Invoria.Ordering.Contracts.Orders.Models;

namespace Invoria.Ordering.Application.Orders.Extensions;

public static class OrderIntegrationEventMappings
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

    public static CreateImmediateReturnIntegrationEvent ToCreateImmediateReturnIntegrationEvent(
        this OrderReturnRequestedIntegrationEvent message) =>
        new()
        {
            OrderId = message.OrderId,
            AllocationId = message.AllocationId,
            Lines = message.Lines
                .Select(l => new ReturnLineModel
                {
                    OrderItemId = l.OrderItemId,
                    ProductId = l.ProductId,
                    Quantity = l.Quantity
                })
                .ToList()
        };
}
