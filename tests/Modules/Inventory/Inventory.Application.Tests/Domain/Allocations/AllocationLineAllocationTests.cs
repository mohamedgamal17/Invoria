using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Inventory.Domain.Allocations;
using Invoria.Inventory.Domain.Batches;

namespace Invoria.Inventory.Application.Tests.Domain.Allocations;

[TestFixture]
public class AllocationLineAllocationTests
{
    [Test]
    public void RecordBatchAllocation_adds_allocation_and_links_to_line()
    {
        var line = new AllocationLine("line-1", "alloc-1", "oi-1", "p-1", 5);
        var batchAllocation = new BatchAllocation("batch-1", "oi-1", 3, DateTimeOffset.UtcNow);

        line.RecordBatchAllocation(batchAllocation);

        line.BatchAllocations.Should().ContainSingle().Which.Should().Be(batchAllocation);
        batchAllocation.AllocationLineId.Should().Be("line-1");
        line.QuantityAllocated.Should().Be(3);
        line.IsFullyAllocated.Should().BeFalse();
    }

    [Test]
    public void MarkAsAllocated_sets_status_when_fully_allocated()
    {
        var line = new AllocationLine("line-1", "alloc-1", "oi-1", "p-1", 5);
        var batch = new Batch("p-1", 10, 10m);
        SetEntityId(batch, "batch-1");

        line.RecordBatchAllocation(batch.AllocateForOrder("oi-1", 3, DateTimeOffset.UtcNow));
        line.RecordBatchAllocation(batch.AllocateForOrder("oi-1", 2, DateTimeOffset.UtcNow));

        line.MarkAsAllocated();

        line.Status.Should().Be(AllocationLineStatus.Allocated);
        line.IsFullyAllocated.Should().BeTrue();
        line.QuantityAllocated.Should().Be(5);
    }

    [Test]
    public void MarkAsAllocated_throws_when_not_fully_allocated()
    {
        var line = new AllocationLine("line-1", "alloc-1", "oi-1", "p-1", 5);
        var batch = new Batch("p-1", 10, 10m);
        SetEntityId(batch, "batch-1");
        line.RecordBatchAllocation(batch.AllocateForOrder("oi-1", 3, DateTimeOffset.UtcNow));

        var act = () => line.MarkAsAllocated();

        act.Should().Throw<InvalidOperationException>();
        line.Status.Should().Be(AllocationLineStatus.Pending);
    }

    [Test]
    public void RecordBatchAllocation_throws_when_line_not_pending()
    {
        var line = new AllocationLine("line-1", "alloc-1", "oi-1", "p-1", 1);
        var batch = new Batch("p-1", 10, 10m);
        SetEntityId(batch, "batch-1");
        line.RecordBatchAllocation(batch.AllocateForOrder("oi-1", 1, DateTimeOffset.UtcNow));
        line.MarkAsAllocated();

        var act = () => line.RecordBatchAllocation(
            new BatchAllocation("batch-2", "oi-1", 1, DateTimeOffset.UtcNow));

        act.Should().Throw<InvalidOperationException>();
    }

    private static void SetEntityId(Batch batch, string id) =>
        typeof(Entity).GetProperty(nameof(Entity.Id))!.SetValue(batch, id);
}
