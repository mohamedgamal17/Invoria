using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Inventory.Contracts.Allocations.Events;

namespace Invoria.Ordering.Application.OrderAllocationConsumptions.Commands.CreateOrderAllocationConsumption;

public sealed class CreateOrderAllocationConsumptionCommand : ICommand<Empty>
{
    public string OrderId { get; init; } = string.Empty;

    public string AllocationId { get; init; } = string.Empty;

    public List<Line> Lines { get; init; } = [];

    public sealed class Line
    {
        public string OrderItemId { get; init; } = string.Empty;

        public string ProductId { get; init; } = string.Empty;

        public int QuantityRequested { get; init; }

        public List<Batch> BatchAllocations { get; init; } = [];
    }

    public sealed class Batch
    {
        public string BatchId { get; init; } = string.Empty;

        public int Quantity { get; init; }

        public decimal UnitPrice { get; init; }
    }

    public static CreateOrderAllocationConsumptionCommand FromEvent(OrderAllocationConsumptionIntegrationEvent message)
    {
        var lines = message.Lines
            .Select(line => new Line
            {
                OrderItemId = line.OrderItemId,
                ProductId = line.ProductId,
                QuantityRequested = line.QuantityRequested,
                BatchAllocations = line.BatchAllocations
                    .Select(batch => new Batch
                    {
                        BatchId = batch.BatchId,
                        Quantity = batch.Quantity,
                        UnitPrice = batch.UnitPrice
                    })
                    .ToList()
            })
            .ToList();

        return new CreateOrderAllocationConsumptionCommand
        {
            OrderId = message.OrderId,
            AllocationId = message.AllocationId,
            Lines = lines
        };
    }
}
