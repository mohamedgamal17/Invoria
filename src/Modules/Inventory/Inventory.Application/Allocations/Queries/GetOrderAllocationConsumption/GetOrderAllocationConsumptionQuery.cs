using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.Inventory.Contracts.Allocations.Events;

namespace Invoria.Inventory.Application.Allocations.Queries.GetOrderAllocationConsumption;

public sealed class GetOrderAllocationConsumptionQuery : IQuery<OrderAllocationConsumptionIntegrationEvent>
{
    public string AllocationId { get; init; } = string.Empty;
}
