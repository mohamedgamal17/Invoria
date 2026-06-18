using Ardalis.GuardClauses;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Inventory.Domain.Allocations.Events;

namespace Invoria.Inventory.Domain.Allocations;

public class Allocation : AuditedAggregateRoot
{
    private readonly List<AllocationLine> _lines = new();

    public string OrderId { get; private set; } = null!;

    public AllocationStatus Status { get; private set; }

    public IReadOnlyCollection<AllocationLine> Lines => _lines.AsReadOnly();

    private Allocation()
    {
    }

    private Allocation(
        string orderId,
        IEnumerable<(string OrderItemId, string ProductId, int QuantityRequested)> lines)
    {
        Guard.Against.NullOrWhiteSpace(orderId);
        Guard.Against.OutOfRange(orderId.Length, nameof(orderId), 1, AllocationTableConsts.OrderIdMaxLength);

        var lineInputs = lines?.ToList() ?? [];
        Guard.Against.NullOrEmpty(lineInputs);

        Id = Guid.NewGuid().ToString();
        OrderId = orderId;
        Status = AllocationStatus.Pending;

        foreach (var (orderItemId, productId, quantityRequested) in lineInputs)
        {
            var lineId = Guid.NewGuid().ToString();
            _lines.Add(new AllocationLine(
                lineId,
                Id!,
                orderItemId,
                productId,
                quantityRequested));
        }

        InitiatePendingAllocation();
    }

    public static Allocation CreateForOrder(
        string orderId,
        IEnumerable<(string OrderItemId, string ProductId, int QuantityRequested)> lines) =>
        new(orderId, lines);

    private void InitiatePendingAllocation()
    {
        AddDomainEvent(new AllocationInitiatedDomainEvent(this));
    }

    public void MarkAsAllocated()
    {
        if (Status != AllocationStatus.Pending)
        {
            throw new InvalidOperationException(
                $"Allocation {Id} must be in {AllocationStatus.Pending} state to mark as allocated.");
        }

        if (_lines.Any(l => !l.IsFullyAllocated))
        {
            throw new InvalidOperationException(
                $"Allocation {Id} cannot be marked as allocated until all lines are fully allocated.");
        }

        foreach (var line in _lines.Where(l => l.Status == AllocationLineStatus.Pending))
        {
            line.MarkAsAllocated();
        }

        Status = AllocationStatus.Allocated;
    }

    public bool TryMarkAsAllocated()
    {
        if (Status != AllocationStatus.Pending)
        {
            throw new InvalidOperationException(
                $"Allocation {Id} must be in {AllocationStatus.Pending} state to mark as allocated.");
        }

        if (_lines.Any(l => l.Status == AllocationLineStatus.Pending))
        {
            throw new InvalidOperationException(
                $"Allocation {Id} cannot be marked as allocated while any line is still pending.");
        }

        if (!_lines.All(l => l.Status == AllocationLineStatus.Allocated))
        {
            return false;
        }

        Status = AllocationStatus.Allocated;
        AddDomainEvent(new AllocationCompletedDomainEvent(this));
        return true;
    }

    public void MarkAsFailed()
    {
        if (Status != AllocationStatus.Pending)
        {
            throw new InvalidOperationException(
                $"Allocation {Id} must be in {AllocationStatus.Pending} state to mark as failed.");
        }

        Status = AllocationStatus.Failed;
        AddDomainEvent(new AllocationFailedDomainEvent(this));
    }

    public void MarkAsReleased()
    {
        if (Status != AllocationStatus.Allocated)
        {
            throw new InvalidOperationException(
                $"Allocation {Id} must be in {AllocationStatus.Allocated} state to mark as released.");
        }

        foreach (var line in _lines)
        {
            line.MarkAsReleased();
        }

        Status = AllocationStatus.Released;
        AddDomainEvent(new AllocationReleasedDomainEvent(this));
    }
}
