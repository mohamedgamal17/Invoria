using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Inventory.Domain.Allocations;
using Invoria.Inventory.Domain.Allocations.Events;
using Invoria.Inventory.Domain.Allocations.Services;
using Invoria.Inventory.Domain.Batches;

namespace Invoria.Inventory.Application.Tests.Domain.Allocations;

[TestFixture]
public class AllocationDomainServiceCompleteTests
{
    private readonly AllocationDomainService _sut = new();

    [Test]
    public void Complete_settles_batch_reserved_quantity_and_marks_allocation_completed()
    {
        var allocation = CreateAllocatedAllocation(out var batch);
        var quantityBefore = batch.Quantity;
        var reservedBefore = batch.ReservedQuantity;

        _sut.Complete(allocation, new Dictionary<string, Batch> { [batch.Id!] = batch });

        allocation.Status.Should().Be(AllocationStatus.Completed);
        allocation.Lines.Should().OnlyContain(l => l.Status == AllocationLineStatus.Completed);
        batch.Quantity.Should().Be(quantityBefore);
        batch.ReservedQuantity.Should().Be(0);
    }

    [Test]
    public void Complete_raises_AllocationSettledDomainEvent()
    {
        var allocation = CreateAllocatedAllocation(out var batch);
        allocation.ClearDomainEvents();

        _sut.Complete(allocation, new Dictionary<string, Batch> { [batch.Id!] = batch });

        allocation.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<AllocationSettledDomainEvent>();
    }

    [Test]
    public void Complete_is_no_op_when_allocation_already_completed()
    {
        var allocation = CreateAllocatedAllocation(out var batch);
        _sut.Complete(allocation, new Dictionary<string, Batch> { [batch.Id!] = batch });
        var quantityAfterFirst = batch.Quantity;
        var reservedAfterFirst = batch.ReservedQuantity;

        _sut.Complete(allocation, new Dictionary<string, Batch> { [batch.Id!] = batch });

        batch.Quantity.Should().Be(quantityAfterFirst);
        batch.ReservedQuantity.Should().Be(reservedAfterFirst);
        allocation.Status.Should().Be(AllocationStatus.Completed);
    }

    [Test]
    public void Complete_throws_when_allocation_is_pending()
    {
        var allocation = Allocation.CreateForOrder("order-1", [("oi-1", "p-1", 2)]);
        var batch = new Batch("p-1", 10, 10m);
        SetEntityId(batch, "batch-1");

        var act = () => _sut.Complete(allocation, new Dictionary<string, Batch> { [batch.Id!] = batch });

        act.Should().Throw<InvalidOperationException>();
        allocation.Status.Should().Be(AllocationStatus.Pending);
    }

    [Test]
    public void Complete_settles_multiple_batches_correctly()
    {
        var allocation = Allocation.CreateForOrder("order-1", [("oi-1", "p-1", 5)]);
        var line = allocation.Lines.Single();
        var olderBatch = new Batch("p-1", 3, 10m);
        SetEntityId(olderBatch, "batch-old");
        var newerBatch = new Batch("p-1", 5, 10m);
        SetEntityId(newerBatch, "batch-new");
        line.RecordBatchAllocation(olderBatch.AllocateForOrder("oi-1", 3, DateTimeOffset.UtcNow));
        line.RecordBatchAllocation(newerBatch.AllocateForOrder("oi-1", 2, DateTimeOffset.UtcNow));
        line.MarkAsAllocated();
        allocation.MarkAsAllocated();

        _sut.Complete(allocation, new Dictionary<string, Batch>
        {
            [olderBatch.Id!] = olderBatch,
            [newerBatch.Id!] = newerBatch,
        });

        allocation.Status.Should().Be(AllocationStatus.Completed);
        olderBatch.ReservedQuantity.Should().Be(0);
        olderBatch.Quantity.Should().Be(0);
        newerBatch.ReservedQuantity.Should().Be(0);
        newerBatch.Quantity.Should().Be(3);
    }

    private static Allocation CreateAllocatedAllocation(out Batch batch)
    {
        var allocation = Allocation.CreateForOrder("order-1", [("oi-1", "p-1", 4)]);
        var line = allocation.Lines.Single();
        batch = new Batch("p-1", 10, 10m);
        SetEntityId(batch, "batch-1");
        line.RecordBatchAllocation(batch.AllocateForOrder("oi-1", 4, DateTimeOffset.UtcNow));
        line.MarkAsAllocated();
        allocation.MarkAsAllocated();
        return allocation;
    }

    private static void SetEntityId(Batch batch, string id) =>
        typeof(Entity).GetProperty(nameof(Entity.Id))!.SetValue(batch, id);
}
