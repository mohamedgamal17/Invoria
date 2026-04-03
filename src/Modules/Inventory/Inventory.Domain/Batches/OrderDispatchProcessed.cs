using Ardalis.GuardClauses;
using Invoria.BuildingBlocks.Domain.Entities;

namespace Invoria.Inventory.Domain.Batches;

/// <summary>
/// Records that inventory dispatch processing completed for an order (idempotency for order-dispatched integration event replays).
/// </summary>
public sealed class OrderDispatchProcessed : Entity
{
    public DateTimeOffset ProcessedAt { get; private set; }

    private OrderDispatchProcessed()
    {
    }

    public OrderDispatchProcessed(string orderId, DateTimeOffset processedAt)
    {
        Guard.Against.NullOrWhiteSpace(orderId);
        Guard.Against.OutOfRange(orderId.Length, nameof(orderId), 1, OrderDispatchProcessedTableConsts.OrderIdMaxLength);

        Id = orderId;
        ProcessedAt = processedAt;
    }
}
