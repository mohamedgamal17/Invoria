using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Inventory.Domain.Allocations;
using Invoria.Inventory.Domain.Allocations.Events;
using Invoria.Inventory.Domain.Batches;

namespace Invoria.Inventory.Application.Tests.Domain.Allocations;

[TestFixture]
public class AllocationMarkAllocatedTests
{
    [Test]
    public void MarkAsAllocated_sets_status_when_all_lines_fully_allocated()
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

        allocation.Status.Should().Be(AllocationStatus.Allocated);
    }

    [Test]
    public void MarkAsAllocated_throws_when_any_line_not_fully_allocated()
    {
        var allocation = Allocation.CreateForOrder("order-1", [("oi-1", "p-1", 5)]);
        var line = allocation.Lines.Single();
        var batch = new Batch("p-1", 10, 10m);
        SetEntityId(batch, "batch-1");
        line.RecordBatchAllocation(batch.AllocateForOrder("oi-1", 3, DateTimeOffset.UtcNow));

        var act = () => allocation.MarkAsAllocated();

        act.Should().Throw<InvalidOperationException>();
        allocation.Status.Should().Be(AllocationStatus.Pending);
    }

    [Test]
    public void MarkAsAllocated_throws_when_allocation_not_pending()
    {
        var allocation = Allocation.CreateForOrder("order-1", [("oi-1", "p-1", 1)]);
        var line = allocation.Lines.Single();
        var batch = new Batch("p-1", 10, 10m);
        SetEntityId(batch, "batch-1");
        line.RecordBatchAllocation(batch.AllocateForOrder("oi-1", 1, DateTimeOffset.UtcNow));
        line.MarkAsAllocated();
        allocation.MarkAsAllocated();

        var act = () => allocation.MarkAsAllocated();

        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void TryMarkAsAllocated_returns_true_when_all_lines_allocated()
    {
        var allocation = Allocation.CreateForOrder("order-1", [("oi-1", "p-1", 2)]);
        var line = allocation.Lines.Single();
        var batch = new Batch("p-1", 10, 10m);
        SetEntityId(batch, "batch-1");
        line.RecordBatchAllocation(batch.AllocateForOrder("oi-1", 2, DateTimeOffset.UtcNow));
        line.MarkAsAllocated();
        allocation.ClearDomainEvents();

        allocation.TryMarkAsAllocated().Should().BeTrue();
        allocation.Status.Should().Be(AllocationStatus.Allocated);
        allocation.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<AllocationCompletedDomainEvent>();
        var completed = (AllocationCompletedDomainEvent)allocation.DomainEvents.Single();
        completed.Allocation.Should().BeSameAs(allocation);
        completed.Allocation.OrderId.Should().Be("order-1");
    }

    [Test]
    public void TryMarkAsAllocated_returns_false_when_any_line_failed()
    {
        var allocation = Allocation.CreateForOrder("order-1", [("oi-1", "p-1", 5)]);
        var line = allocation.Lines.Single();
        var batch = new Batch("p-1", 10, 10m);
        SetEntityId(batch, "batch-1");
        line.RecordBatchAllocation(batch.AllocateForOrder("oi-1", 3, DateTimeOffset.UtcNow));
        line.MarkAsFailed();

        allocation.TryMarkAsAllocated().Should().BeFalse();
        allocation.Status.Should().Be(AllocationStatus.Pending);
    }

    [Test]
    public void MarkAsFailed_raises_AllocationFailedDomainEvent()
    {
        var allocation = Allocation.CreateForOrder("order-9", [("oi-1", "p-1", 3)]);
        allocation.ClearDomainEvents();

        allocation.MarkAsFailed();

        allocation.Status.Should().Be(AllocationStatus.Failed);
        allocation.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<AllocationFailedDomainEvent>();
        var failed = (AllocationFailedDomainEvent)allocation.DomainEvents.Single();
        failed.Allocation.Should().BeSameAs(allocation);
        failed.Allocation.OrderId.Should().Be("order-9");
    }

    private static void SetEntityId(Batch batch, string id) =>
        typeof(Entity).GetProperty(nameof(Entity.Id))!.SetValue(batch, id);
}
