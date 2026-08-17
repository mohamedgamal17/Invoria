using Ardalis.GuardClauses;
using Invoria.BuildingBlocks.Domain.Events;

namespace Invoria.Ordering.Domain.OrderAllocationConsumptions.Events;

public sealed class OrderAllocationConsumptionCreatedDomainEvent : DomainEvent
{
    public OrderAllocationConsumptionCreatedDomainEvent(OrderAllocationConsumption consumption)
    {
        Consumption = Guard.Against.Null(consumption);
        OrderId = consumption.OrderId;
        AllocationId = consumption.AllocationId;
    }

    public string OrderId { get; }

    public string AllocationId { get; }

    public OrderAllocationConsumption Consumption { get; }
}
