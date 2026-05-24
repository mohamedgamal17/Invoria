using Ardalis.GuardClauses;
using Invoria.BuildingBlocks.Domain.Entities;

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

    public static Allocation CreateForOrder(
        string orderId,
        IEnumerable<(string OrderItemId, string ProductId, int QuantityRequested)> lines)
    {
        Guard.Against.NullOrWhiteSpace(orderId);
        Guard.Against.OutOfRange(orderId.Length, nameof(orderId), 1, AllocationTableConsts.OrderIdMaxLength);

        var lineInputs = lines?.ToList() ?? [];
        Guard.Against.NullOrEmpty(lineInputs);

        var allocation = new Allocation
        {
            Id = Guid.NewGuid().ToString(),
            OrderId = orderId,
            Status = AllocationStatus.Pending
        };

        foreach (var (orderItemId, productId, quantityRequested) in lineInputs)
        {
            var lineId = Guid.NewGuid().ToString();
            allocation._lines.Add(new AllocationLine(
                lineId,
                allocation.Id!,
                orderItemId,
                productId,
                quantityRequested));
        }

        return allocation;
    }
}
