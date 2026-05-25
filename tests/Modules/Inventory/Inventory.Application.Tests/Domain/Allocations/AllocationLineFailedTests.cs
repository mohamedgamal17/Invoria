using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Inventory.Domain.Allocations;
using Invoria.Inventory.Domain.Batches;

namespace Invoria.Inventory.Application.Tests.Domain.Allocations;

[TestFixture]
public class AllocationLineFailedTests
{
    [Test]
    public void MarkAsFailed_sets_status_when_partially_allocated()
    {
        var line = new AllocationLine("line-1", "alloc-1", "oi-1", "p-1", 5);
        var batch = new Batch("p-1", 10, 10m);
        SetEntityId(batch, "batch-1");
        line.RecordBatchAllocation(batch.AllocateForOrder("oi-1", 3, DateTimeOffset.UtcNow));

        line.MarkAsFailed();

        line.Status.Should().Be(AllocationLineStatus.Failed);
        line.BatchAllocations.Should().ContainSingle();
        line.QuantityAllocated.Should().Be(3);
    }

    [Test]
    public void MarkAsFailed_sets_status_when_no_batch_allocations()
    {
        var line = new AllocationLine("line-1", "alloc-1", "oi-1", "p-1", 5);

        line.MarkAsFailed();

        line.Status.Should().Be(AllocationLineStatus.Failed);
        line.BatchAllocations.Should().BeEmpty();
    }

    [Test]
    public void MarkAsFailed_throws_when_fully_allocated()
    {
        var line = new AllocationLine("line-1", "alloc-1", "oi-1", "p-1", 2);
        var batch = new Batch("p-1", 10, 10m);
        SetEntityId(batch, "batch-1");
        line.RecordBatchAllocation(batch.AllocateForOrder("oi-1", 2, DateTimeOffset.UtcNow));

        var act = () => line.MarkAsFailed();

        act.Should().Throw<InvalidOperationException>();
    }

    private static void SetEntityId(Batch batch, string id) =>
        typeof(Entity).GetProperty(nameof(Entity.Id))!.SetValue(batch, id);
}
