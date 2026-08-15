using Ardalis.GuardClauses;
using Invoria.BuildingBlocks.Domain.Entities;

namespace Invoria.Ordering.Domain.OrderAllocationConsumptions;

public class OrderAllocationConsumptionBatch : AuditedEntity
{
    public string ConsumptionLineId { get; private set; } = null!;

    public string BatchId { get; private set; } = null!;

    public int Quantity { get; private set; }

    public decimal UnitPrice { get; private set; }

    private OrderAllocationConsumptionBatch()
    {
    }

    public OrderAllocationConsumptionBatch(
        string id,
        string batchId,
        int quantity,
        decimal unitPrice)
    {
        Guard.Against.NullOrWhiteSpace(id);
        Guard.Against.OutOfRange(id.Length, nameof(id), 1, OrderAllocationConsumptionBatchTableConsts.IdMaxLength);
        Guard.Against.NullOrWhiteSpace(batchId);
        Guard.Against.OutOfRange(batchId.Length, nameof(batchId), 1, OrderAllocationConsumptionBatchTableConsts.BatchIdMaxLength);
        Guard.Against.NegativeOrZero(quantity);
        Guard.Against.NegativeOrZero(unitPrice);

        Id = id;
        BatchId = batchId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public void AttachToLine(string consumptionLineId)
    {
        Guard.Against.NullOrWhiteSpace(consumptionLineId);
        Guard.Against.OutOfRange(consumptionLineId.Length, nameof(consumptionLineId), 1, OrderAllocationConsumptionBatchTableConsts.ConsumptionLineIdMaxLength);

        ConsumptionLineId = consumptionLineId;
    }
}
