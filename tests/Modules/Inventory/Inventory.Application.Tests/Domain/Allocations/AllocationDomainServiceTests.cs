using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Inventory.Domain.Allocations;
using Invoria.Inventory.Domain.Allocations.Events;
using Invoria.Inventory.Domain.Allocations.Services;
using Invoria.Inventory.Domain.Batches;

namespace Invoria.Inventory.Application.Tests.Domain.Allocations;

[TestFixture]
public class AllocationDomainServiceTests
{
    private readonly AllocationDomainService _sut = new();

    [Test]
    public void Allocate_marks_allocation_and_line_when_single_batch_has_sufficient_stock()
    {
        var orderItemId = "oi-1";
        var productId = "p-1";
        var allocation = Allocation.CreateForOrder("order-1", [(orderItemId, productId, 4)]);
        var batch = new Batch(productId, 10, 10m);
        SetEntityId(batch, "batch-1");

        var batchesByProduct = new Dictionary<string, List<Batch>>
        {
            [productId] = [batch],
        };

        _sut.Allocate(allocation, batchesByProduct);

        allocation.Status.Should().Be(AllocationStatus.Allocated);
        var line = allocation.Lines.Single();
        line.Status.Should().Be(AllocationLineStatus.Allocated);
        line.BatchAllocations.Should().ContainSingle();
        line.BatchAllocations.Single().QuantityAllocated.Should().Be(4);
        batch.Quantity.Should().Be(6);
        batch.ReservedQuantity.Should().Be(4);
        allocation.DomainEvents.Should().ContainSingle(e => e is AllocationCompletedDomainEvent);
    }

    [Test]
    public void Allocate_consumes_older_batch_first_when_multiple_batches_exist()
    {
        var orderItemId = "oi-1";
        var productId = "p-1";
        var allocation = Allocation.CreateForOrder("order-1", [(orderItemId, productId, 5)]);

        var olderBatch = new Batch(productId, 3, 10m);
        SetEntityId(olderBatch, "batch-old");
        var newerBatch = new Batch(productId, 5, 10m);
        SetEntityId(newerBatch, "batch-new");

        var batchesByProduct = new Dictionary<string, List<Batch>>
        {
            [productId] = [olderBatch, newerBatch],
        };

        _sut.Allocate(allocation, batchesByProduct);

        allocation.Status.Should().Be(AllocationStatus.Allocated);
        var line = allocation.Lines.Single();
        line.BatchAllocations.Should().HaveCount(2);
        line.BatchAllocations.Should().Contain(a => a.BatchId == "batch-old" && a.QuantityAllocated == 3);
        line.BatchAllocations.Should().Contain(a => a.BatchId == "batch-new" && a.QuantityAllocated == 2);
        olderBatch.Quantity.Should().Be(0);
        newerBatch.Quantity.Should().Be(3);
    }

    [Test]
    public void Allocate_marks_line_and_allocation_failed_when_insufficient_stock()
    {
        var allocation = Allocation.CreateForOrder("order-1", [("oi-1", "p-1", 10)]);
        var batch = new Batch("p-1", 4, 10m);
        SetEntityId(batch, "batch-1");

        _sut.Allocate(allocation, new Dictionary<string, List<Batch>> { ["p-1"] = [batch] });

        allocation.Status.Should().Be(AllocationStatus.Failed);
        allocation.Lines.Single().Status.Should().Be(AllocationLineStatus.Failed);
        batch.Quantity.Should().Be(4);
        batch.ReservedQuantity.Should().Be(0);
        allocation.DomainEvents.Should().ContainSingle(e => e is AllocationFailedDomainEvent);
    }

    [Test]
    public void Allocate_marks_line_failed_when_product_missing_from_dictionary()
    {
        var allocation = Allocation.CreateForOrder(
            "order-1",
            [("oi-1", "p-missing", 2), ("oi-2", "p-1", 1)]);

        var batch = new Batch("p-1", 5, 10m);
        SetEntityId(batch, "batch-1");

        _sut.Allocate(allocation, new Dictionary<string, List<Batch>> { ["p-1"] = [batch] });

        allocation.Status.Should().Be(AllocationStatus.Failed);
        allocation.Lines.Single(l => l.ProductId == "p-missing").Status.Should().Be(AllocationLineStatus.Failed);
        allocation.Lines.Single(l => l.ProductId == "p-1").Status.Should().Be(AllocationLineStatus.Released);
        batch.Quantity.Should().Be(5);
        batch.ReservedQuantity.Should().Be(0);
    }

    [Test]
    public void Allocate_releases_reserved_stock_when_allocation_fails_after_partial_reserve()
    {
        var allocation = Allocation.CreateForOrder(
            "order-1",
            [("oi-1", "p-1", 10), ("oi-2", "p-2", 1)]);

        var batchP1 = new Batch("p-1", 4, 10m);
        SetEntityId(batchP1, "batch-p1");
        var batchP2 = new Batch("p-2", 5, 10m);
        SetEntityId(batchP2, "batch-p2");

        _sut.Allocate(allocation, new Dictionary<string, List<Batch>>
        {
            ["p-1"] = [batchP1],
            ["p-2"] = [batchP2],
        });

        allocation.Status.Should().Be(AllocationStatus.Failed);
        batchP1.Quantity.Should().Be(4);
        batchP1.ReservedQuantity.Should().Be(0);
        allocation.Lines.Single(l => l.ProductId == "p-1").Status.Should().Be(AllocationLineStatus.Failed);
        allocation.Lines.Single(l => l.ProductId == "p-2").Status.Should().Be(AllocationLineStatus.Released);
    }

    private static void SetEntityId(Batch batch, string id) =>
        typeof(Entity).GetProperty(nameof(Entity.Id))!.SetValue(batch, id);
}
