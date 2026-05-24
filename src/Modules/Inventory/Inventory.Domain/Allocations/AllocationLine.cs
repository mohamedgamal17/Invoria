using Ardalis.GuardClauses;
using Invoria.BuildingBlocks.Domain.Entities;

namespace Invoria.Inventory.Domain.Allocations;

public class AllocationLine : AuditedEntity
{
    public string AllocationId { get; private set; } = null!;

    public string OrderItemId { get; private set; } = null!;

    public string ProductId { get; private set; } = null!;

    public int QuantityRequested { get; private set; }

    public AllocationLineStatus Status { get; private set; }

    public ICollection<BatchAllocation> BatchAllocations { get; private set; } = null!;

    private AllocationLine()
    {
        BatchAllocations = new HashSet<BatchAllocation>();
    }

    public AllocationLine(
        string id,
        string allocationId,
        string orderItemId,
        string productId,
        int quantityRequested)
    {
        Guard.Against.NullOrWhiteSpace(id);
        Guard.Against.OutOfRange(id.Length, nameof(id), 1, AllocationLineTableConsts.IdMaxLength);
        Guard.Against.NullOrWhiteSpace(allocationId);
        Guard.Against.OutOfRange(allocationId.Length, nameof(allocationId), 1, AllocationLineTableConsts.AllocationIdMaxLength);
        Guard.Against.NullOrWhiteSpace(orderItemId);
        Guard.Against.OutOfRange(orderItemId.Length, nameof(orderItemId), 1, AllocationLineTableConsts.OrderItemIdMaxLength);
        Guard.Against.NullOrWhiteSpace(productId);
        Guard.Against.OutOfRange(productId.Length, nameof(productId), 1, AllocationLineTableConsts.ProductIdMaxLength);
        Guard.Against.NegativeOrZero(quantityRequested);

        Id = id;
        AllocationId = allocationId;
        OrderItemId = orderItemId;
        ProductId = productId;
        QuantityRequested = quantityRequested;
        Status = AllocationLineStatus.Pending;
        BatchAllocations = new HashSet<BatchAllocation>();
    }
}
