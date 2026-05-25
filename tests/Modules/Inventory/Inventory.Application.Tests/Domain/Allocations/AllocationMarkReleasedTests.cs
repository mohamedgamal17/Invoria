using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Inventory.Domain.Allocations;
using Invoria.Inventory.Domain.Batches;

namespace Invoria.Inventory.Application.Tests.Domain.Allocations;

[TestFixture]
public class AllocationMarkReleasedTests
{
    [Test]
    public void MarkAsReleased_sets_status_when_all_lines_allocated()
    {
        var allocation = CreateAllocatedAllocation();

        allocation.MarkAsReleased();

        allocation.Status.Should().Be(AllocationStatus.Released);
        allocation.Lines.Should().OnlyContain(l => l.Status == AllocationLineStatus.Released);
    }

    [Test]
    public void MarkAsReleased_throws_when_not_allocated()
    {
        var allocation = Allocation.CreateForOrder("order-1", [("oi-1", "p-1", 1)]);

        var act = () => allocation.MarkAsReleased();

        act.Should().Throw<InvalidOperationException>();
        allocation.Status.Should().Be(AllocationStatus.Pending);
    }

    [Test]
    public void MarkAsReleased_throws_when_any_line_not_allocated()
    {
        var allocation = CreateAllocatedAllocation();
        var pendingLine = allocation.Lines.First();
        SetLineStatus(pendingLine, AllocationLineStatus.Pending);

        var act = () => allocation.MarkAsReleased();

        act.Should().Throw<InvalidOperationException>();
        allocation.Status.Should().Be(AllocationStatus.Allocated);
        pendingLine.Status.Should().Be(AllocationLineStatus.Pending);
    }

    private static Allocation CreateAllocatedAllocation()
    {
        var allocation = Allocation.CreateForOrder(
            "order-1",
            [("oi-1", "p-1", 3), ("oi-2", "p-2", 2)]);

        var batch1 = new Batch("p-1", 10, 10m);
        SetEntityId(batch1, "batch-1");
        var batch2 = new Batch("p-2", 10, 10m);
        SetEntityId(batch2, "batch-2");

        foreach (var line in allocation.Lines)
        {
            var batch = line.ProductId == "p-1" ? batch1 : batch2;
            line.RecordBatchAllocation(
                batch.AllocateForOrder(line.OrderItemId, line.QuantityRequested, DateTimeOffset.UtcNow));
            line.MarkAsAllocated();
        }

        allocation.MarkAsAllocated();
        return allocation;
    }

    private static void SetEntityId(Batch batch, string id) =>
        typeof(Entity).GetProperty(nameof(Entity.Id))!.SetValue(batch, id);

    private static void SetLineStatus(AllocationLine line, AllocationLineStatus status) =>
        typeof(AllocationLine).GetProperty(nameof(AllocationLine.Status))!.SetValue(line, status);
}
