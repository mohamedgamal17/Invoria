using Ardalis.GuardClauses;
using Invoria.BuildingBlocks.Domain.Entities;

namespace Invoria.Ordering.Domain.OrderAllocationConsumptions;

public class OrderAllocationConsumptionLine : AuditedEntity
{
    private readonly List<OrderAllocationConsumptionBatch> _batchAllocations = new();

    public string ConsumptionId { get; private set; } = null!;

    public string OrderItemId { get; private set; } = null!;

    public string ProductId { get; private set; } = null!;

    public int QuantityRequested { get; private set; }

    public IReadOnlyCollection<OrderAllocationConsumptionBatch> BatchAllocations => _batchAllocations.AsReadOnly();

    private OrderAllocationConsumptionLine()
    {
    }

    public OrderAllocationConsumptionLine(
        string id,
        string orderItemId,
        string productId,
        int quantityRequested)
    {
        Guard.Against.NullOrWhiteSpace(id);
        Guard.Against.OutOfRange(id.Length, nameof(id), 1, OrderAllocationConsumptionLineTableConsts.IdMaxLength);
        Guard.Against.NullOrWhiteSpace(orderItemId);
        Guard.Against.OutOfRange(orderItemId.Length, nameof(orderItemId), 1, OrderAllocationConsumptionLineTableConsts.OrderItemIdMaxLength);
        Guard.Against.NullOrWhiteSpace(productId);
        Guard.Against.OutOfRange(productId.Length, nameof(productId), 1, OrderAllocationConsumptionLineTableConsts.ProductIdMaxLength);
        Guard.Against.NegativeOrZero(quantityRequested);

        Id = id;
        OrderItemId = orderItemId;
        ProductId = productId;
        QuantityRequested = quantityRequested;
    }

    public void AddBatchAllocation(OrderAllocationConsumptionBatch batchAllocation)
    {
        Guard.Against.Null(batchAllocation);

        _batchAllocations.Add(batchAllocation);
    }

    public void AttachToConsumption(string consumptionId)
    {
        Guard.Against.NullOrWhiteSpace(consumptionId);
        Guard.Against.OutOfRange(consumptionId.Length, nameof(consumptionId), 1, OrderAllocationConsumptionLineTableConsts.ConsumptionIdMaxLength);

        ConsumptionId = consumptionId;

        foreach (var batchAllocation in _batchAllocations)
        {
            batchAllocation.AttachToLine(Id!);
        }
    }
}
