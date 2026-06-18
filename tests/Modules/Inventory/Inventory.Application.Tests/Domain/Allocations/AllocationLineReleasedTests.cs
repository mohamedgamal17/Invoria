using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Inventory.Domain.Allocations;
using Invoria.Inventory.Domain.Batches;

namespace Invoria.Inventory.Application.Tests.Domain.Allocations;

[TestFixture]
public class AllocationLineReleasedTests
{
    [Test]
    public void MarkAsReleased_sets_status_from_allocated()
    {
        var line = new AllocationLine("line-1", "alloc-1", "oi-1", "p-1", 2);
        var batch = new Batch("p-1", 10, 10m);
        SetEntityId(batch, "batch-1");
        line.RecordBatchAllocation(batch.AllocateForOrder("oi-1", 2, DateTimeOffset.UtcNow));
        line.MarkAsAllocated();

        line.MarkAsReleased();

        line.Status.Should().Be(AllocationLineStatus.Released);
        line.BatchAllocations.Should().ContainSingle();
    }

    [Test]
    public void MarkAsReleased_throws_when_not_allocated()
    {
        var line = new AllocationLine("line-1", "alloc-1", "oi-1", "p-1", 2);

        var act = () => line.MarkAsReleased();

        act.Should().Throw<InvalidOperationException>();
    }

    private static void SetEntityId(Batch batch, string id) =>
        typeof(Entity).GetProperty(nameof(Entity.Id))!.SetValue(batch, id);
}
