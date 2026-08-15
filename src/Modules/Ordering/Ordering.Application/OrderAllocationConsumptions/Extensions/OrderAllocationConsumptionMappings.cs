using Invoria.Ordering.Application.OrderAllocationConsumptions.Commands.CreateOrderAllocationConsumption;
using Invoria.Ordering.Domain.OrderAllocationConsumptions;

namespace Invoria.Ordering.Application.OrderAllocationConsumptions.Extensions;

public static class OrderAllocationConsumptionMappings
{
    public static OrderAllocationConsumption ToOrderAllocationConsumption(
        this CreateOrderAllocationConsumptionCommand command)
    {
        var lines = command.Lines.Select(ToLine).ToList();

        return OrderAllocationConsumption.Create(
            command.OrderId,
            command.AllocationId,
            lines);
    }

    private static OrderAllocationConsumptionLine ToLine(CreateOrderAllocationConsumptionCommand.Line line)
    {
        var consumptionLine = new OrderAllocationConsumptionLine(
            Guid.NewGuid().ToString("N"),
            line.OrderItemId,
            line.ProductId,
            line.QuantityRequested);

        foreach (var batch in line.BatchAllocations)
        {
            var batchAllocation = new OrderAllocationConsumptionBatch(
                Guid.NewGuid().ToString("N"),
                batch.BatchId,
                batch.Quantity,
                batch.UnitPrice);

            consumptionLine.AddBatchAllocation(batchAllocation);
        }

        return consumptionLine;
    }
}
