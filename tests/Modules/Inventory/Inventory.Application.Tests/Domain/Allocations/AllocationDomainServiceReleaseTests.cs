using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Inventory.Domain.Allocations;
using Invoria.Inventory.Domain.Allocations.Services;
using Invoria.Inventory.Domain.Batches;

namespace Invoria.Inventory.Application.Tests.Domain.Allocations;

[TestFixture]
public class AllocationDomainServiceReleaseTests
{
    private readonly AllocationDomainService _sut = new();

    [Test]
    public void Release_restores_batch_stock_and_marks_allocation_released()
    {
        var allocation = CreateAllocatedAllocation(out var batch);
        var quantityBefore = batch.Quantity;
        var reservedBefore = batch.ReservedQuantity;

        _sut.Release(allocation, new Dictionary<string, Batch> { [batch.Id!] = batch });

        allocation.Status.Should().Be(AllocationStatus.Released);
        allocation.Lines.Should().OnlyContain(l => l.Status == AllocationLineStatus.Released);
        batch.Quantity.Should().Be(quantityBefore + reservedBefore);
        batch.ReservedQuantity.Should().Be(0);
    }

    [Test]
    public void Release_is_no_op_when_allocation_already_released()
    {
        var allocation = CreateAllocatedAllocation(out var batch);
        _sut.Release(allocation, new Dictionary<string, Batch> { [batch.Id!] = batch });
        var quantityAfterFirst = batch.Quantity;
        var reservedAfterFirst = batch.ReservedQuantity;

        _sut.Release(allocation, new Dictionary<string, Batch> { [batch.Id!] = batch });

        batch.Quantity.Should().Be(quantityAfterFirst);
        batch.ReservedQuantity.Should().Be(reservedAfterFirst);
        allocation.Status.Should().Be(AllocationStatus.Released);
    }

    [Test]
    public void Release_throws_when_allocation_is_pending()
    {
        var allocation = Allocation.CreateForOrder("order-1", [("oi-1", "p-1", 2)]);
        var batch = new Batch("p-1", 10, 10m);
        SetEntityId(batch, "batch-1");

        var act = () => _sut.Release(allocation, new Dictionary<string, Batch> { [batch.Id!] = batch });

        act.Should().Throw<InvalidOperationException>();
        allocation.Status.Should().Be(AllocationStatus.Pending);
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
