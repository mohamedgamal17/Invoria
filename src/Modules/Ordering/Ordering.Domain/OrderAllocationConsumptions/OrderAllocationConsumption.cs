using Ardalis.GuardClauses;
using Invoria.BuildingBlocks.Domain.Entities;

namespace Invoria.Ordering.Domain.OrderAllocationConsumptions;

public class OrderAllocationConsumption : AuditedAggregateRoot
{
    private readonly List<OrderAllocationConsumptionLine> _lines = new();

    public string OrderId { get; private set; } = null!;

    public string AllocationId { get; private set; } = null!;

    public IReadOnlyCollection<OrderAllocationConsumptionLine> Lines => _lines.AsReadOnly();

    private OrderAllocationConsumption()
    {
    }

    public static OrderAllocationConsumption Create(
        string orderId,
        string allocationId,
        IEnumerable<OrderAllocationConsumptionLine> lines)
    {
        Guard.Against.NullOrWhiteSpace(orderId);
        Guard.Against.OutOfRange(orderId.Length, nameof(orderId), 1, OrderAllocationConsumptionTableConsts.OrderIdMaxLength);
        Guard.Against.NullOrWhiteSpace(allocationId);
        Guard.Against.OutOfRange(allocationId.Length, nameof(allocationId), 1, OrderAllocationConsumptionTableConsts.AllocationIdMaxLength);

        var lineList = lines?.ToList() ?? [];
        Guard.Against.NullOrEmpty(lineList);

        var consumption = new OrderAllocationConsumption
        {
            Id = Guid.NewGuid().ToString("N"),
            OrderId = orderId,
            AllocationId = allocationId
        };

        foreach (var line in lineList)
        {
            line.AttachToConsumption(consumption.Id!);
            consumption._lines.Add(line);
        }

        return consumption;
    }
}
