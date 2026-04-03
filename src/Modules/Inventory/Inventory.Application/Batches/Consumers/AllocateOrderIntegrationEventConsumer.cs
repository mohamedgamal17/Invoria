using Invoria.Ordering.Contracts.Events;
using Invoria.Inventory.Application.Batches.Commands.AllocateOrder;
using Invoria.Inventory.Domain.Batches;
using MediatR;
using Rebus.Bus;
using Rebus.Handlers;

namespace Invoria.Inventory.Application.Batches.Consumers;

public sealed class AllocateOrderIntegrationEventConsumer : IHandleMessages<AllocateOrderIntegrationEvent>
{
    private readonly IMediator _mediator;
    private readonly IBus _bus;

    public AllocateOrderIntegrationEventConsumer(IMediator mediator, IBus bus)
    {
        _mediator = mediator;
        _bus = bus;
    }

    public async Task Handle(AllocateOrderIntegrationEvent message)
    {
        var result = await _mediator.Send(AllocateOrderCommand.FromEvent(message), CancellationToken.None);

        if (result.IsSuccess)
        {
            await _bus.Publish(new OrderAllocationSucceededIntegrationEvent
            {
                OrderId = message.Id,
                OrderNumber = message.OrderNumber,
                CustomerId = message.CustomerId,
                AllocatedAt = DateTimeOffset.UtcNow,
                AllocatedLines = message.Items
                    .Select(i => new OrderAllocationSucceededLineModel
                    {
                        OrderItemId = i.Id,
                        ProductId = i.ProductId,
                        Quantity = i.Quantity
                    })
                    .ToList()
            });
            return;
        }

        await _bus.Publish(new OrderAllocationFailedIntegrationEvent
        {
            OrderId = message.Id,
            Reason = result.Exception?.Message ?? "Unknown allocation failure.",
            OrderNumber = message.OrderNumber,
            Details = result.Exception?.ToString(),
            ItemErrors = MapItemErrors(message, result.Exception)
        });
    }

    private static List<OrderAllocationItemErrorModel> MapItemErrors(
        AllocateOrderIntegrationEvent message,
        Exception? exception)
    {
        if (exception is not OrderAllocationPreFlightException preFlight)
        {
            return [];
        }

        var requestedByProduct = message.Items
            .GroupBy(i => i.ProductId)
            .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));

        var firstOrderItemByProduct = message.Items
            .GroupBy(i => i.ProductId)
            .ToDictionary(g => g.Key, g => g.First().Id);

        var itemErrors = new List<OrderAllocationItemErrorModel>();

        foreach (var error in preFlight.Errors)
        {
            itemErrors.Add(new OrderAllocationItemErrorModel
            {
                OrderItemId = firstOrderItemByProduct.TryGetValue(error.ProductId, out var orderItemId) ? orderItemId : "unknown",
                ProductId = error.ProductId,
                RequestedQuantity = requestedByProduct.TryGetValue(error.ProductId, out var rq) ? rq : error.RequestedQuantity,
                AvailableQuantity = error.AvailableQuantity,
                Message = error.Message
            });
        }

        return itemErrors;
    }
}

