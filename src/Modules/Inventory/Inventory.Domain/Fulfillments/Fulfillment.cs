using Ardalis.GuardClauses;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Inventory.Domain.Allocations;

namespace Invoria.Inventory.Domain.Fulfillments;

public class Fulfillment : AuditedAggregateRoot
{
    private readonly List<FulfillmentItem> _items = new();

    public string OrderId { get; private set; } = null!;

    public string AllocationId { get; private set; } = null!;

    public FulfillmentStatus Status { get; private set; }

    public IReadOnlyCollection<FulfillmentItem> Items => _items.AsReadOnly();

    private Fulfillment()
    {
    }

    private Fulfillment(Allocation allocation)
    {
        Guard.Against.Null(allocation);
        Guard.Against.NullOrWhiteSpace(allocation.Id);
        Guard.Against.OutOfRange(allocation.Id!.Length, nameof(allocation), 1, FulfillmentTableConsts.AllocationIdMaxLength);
        Guard.Against.NullOrWhiteSpace(allocation.OrderId);
        Guard.Against.OutOfRange(allocation.OrderId.Length, nameof(allocation), 1, FulfillmentTableConsts.OrderIdMaxLength);

        if (allocation.Status != AllocationStatus.Allocated)
        {
            throw new InvalidOperationException(
                $"Fulfillment can only be created from an allocation in {AllocationStatus.Allocated} state.");
        }

        var lines = allocation.Lines.ToList();
        Guard.Against.NullOrEmpty(lines);

        Id = Guid.NewGuid().ToString();
        OrderId = allocation.OrderId;
        AllocationId = allocation.Id!;
        Status = FulfillmentStatus.Pending;

        foreach (var line in lines)
        {
            _items.Add(new FulfillmentItem(
                Guid.NewGuid().ToString(),
                Id!,
                line.ProductId,
                line.Id!,
                line.QuantityAllocated));
        }
    }

    public static Fulfillment CreateFromAllocation(Allocation allocation) =>
        new(allocation);

    public void MarkInProgress()
    {
        if (Status != FulfillmentStatus.Pending)
        {
            throw new InvalidOperationException(
                $"Fulfillment {Id} must be in {FulfillmentStatus.Pending} state to mark as in progress.");
        }

        Status = FulfillmentStatus.InProgress;
    }

    public void MarkCompleted()
    {
        if (Status != FulfillmentStatus.InProgress)
        {
            throw new InvalidOperationException(
                $"Fulfillment {Id} must be in {FulfillmentStatus.InProgress} state to mark as completed.");
        }

        Status = FulfillmentStatus.Completed;
    }
}
