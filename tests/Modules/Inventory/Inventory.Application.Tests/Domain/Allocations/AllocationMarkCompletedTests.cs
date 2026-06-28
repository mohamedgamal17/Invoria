using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Inventory.Domain.Allocations;
using Invoria.Inventory.Domain.Allocations.Events;
using Invoria.Inventory.Domain.Batches;

namespace Invoria.Inventory.Application.Tests.Domain.Allocations;

[TestFixture]
public class AllocationMarkCompletedTests
{
    [Test]
    public void MarkAsCompleted_sets_status_when_all_lines_allocated()
    {
        var allocation = CreateAllocatedAllocation();

        allocation.MarkAsCompleted();

        allocation.Status.Should().Be(AllocationStatus.Completed);
        allocation.Lines.Should().OnlyContain(l => l.Status == AllocationLineStatus.Completed);
    }

    [Test]
    public void MarkAsCompleted_raises_AllocationSettledDomainEvent_with_allocation_instance()
    {
        var allocation = CreateAllocatedAllocation();
        allocation.ClearDomainEvents();

        allocation.MarkAsCompleted();

        allocation.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<AllocationSettledDomainEvent>()
            .Which.Allocation.Should().BeSameAs(allocation);
    }

    [Test]
    public void MarkAsCompleted_throws_when_not_allocated()
    {
        var allocation = Allocation.CreateForOrder("order-1", [("oi-1", "p-1", 1)]);

        var act = () => allocation.MarkAsCompleted();

        act.Should().Throw<InvalidOperationException>();
        allocation.Status.Should().Be(AllocationStatus.Pending);
    }

    [Test]
    public void MarkAsCompleted_throws_when_allocation_is_released()
    {
        var allocation = CreateAllocatedAllocation();
        allocation.MarkAsReleased();

        var act = () => allocation.MarkAsCompleted();

        act.Should().Throw<InvalidOperationException>();
        allocation.Status.Should().Be(AllocationStatus.Released);
    }

    [Test]
    public void MarkAsCompleted_throws_when_any_line_not_allocated()
    {
        var allocation = CreateAllocatedAllocation();
        var pendingLine = allocation.Lines.First();
        SetLineStatus(pendingLine, AllocationLineStatus.Pending);

        var act = () => allocation.MarkAsCompleted();

        act.Should().Throw<InvalidOperationException>();
        allocation.Status.Should().Be(AllocationStatus.Allocated);
        pendingLine.Status.Should().Be(AllocationLineStatus.Pending);
    }

    [Test]
    public void AllocationLine_MarkAsCompleted_sets_status_when_allocated()
    {
        var line = new AllocationLine("line-1", "alloc-1", "oi-1", "p-1", 5);
        var batch = new Batch("p-1", 10, 10m);
        SetEntityId(batch, "batch-1");
        line.RecordBatchAllocation(batch.AllocateForOrder("oi-1", 5, DateTimeOffset.UtcNow));
        line.MarkAsAllocated();

        line.MarkAsCompleted();

        line.Status.Should().Be(AllocationLineStatus.Completed);
    }

    [Test]
    public void AllocationLine_MarkAsCompleted_throws_when_not_allocated()
    {
        var line = new AllocationLine("line-1", "alloc-1", "oi-1", "p-1", 5);

        var act = () => line.MarkAsCompleted();

        act.Should().Throw<InvalidOperationException>();
        line.Status.Should().Be(AllocationLineStatus.Pending);
    }

    [Test]
    public void AllocationLine_MarkAsCompleted_throws_when_released()
    {
        var line = new AllocationLine("line-1", "alloc-1", "oi-1", "p-1", 5);
        var batch = new Batch("p-1", 10, 10m);
        SetEntityId(batch, "batch-1");
        line.RecordBatchAllocation(batch.AllocateForOrder("oi-1", 5, DateTimeOffset.UtcNow));
        line.MarkAsAllocated();
        line.MarkAsReleased();

        var act = () => line.MarkAsCompleted();

        act.Should().Throw<InvalidOperationException>();
        line.Status.Should().Be(AllocationLineStatus.Released);
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
