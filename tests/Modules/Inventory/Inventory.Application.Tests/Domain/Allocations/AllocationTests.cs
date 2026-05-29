using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Inventory.Domain.Allocations;
using Invoria.Inventory.Domain.Allocations.Events;
using Invoria.Inventory.Domain.Batches;

namespace Invoria.Inventory.Application.Tests.Domain.Allocations;

[TestFixture]
public class AllocationTests
{
    [Test]
    public void CreateForOrder_should_create_allocation_with_pending_status_and_lines()
    {
        var orderId = "order-1";
        var lineInputs = new[]
        {
            ("order-item-1", "product-1", 3),
            ("order-item-2", "product-2", 5)
        };

        var allocation = Allocation.CreateForOrder(orderId, lineInputs);

        allocation.OrderId.Should().Be(orderId);
        allocation.Status.Should().Be(AllocationStatus.Pending);
        allocation.Lines.Should().HaveCount(2);
        allocation.Lines.Should().ContainSingle(l =>
            l.OrderItemId == "order-item-1" && l.ProductId == "product-1" && l.QuantityRequested == 3);
        allocation.Lines.Should().ContainSingle(l =>
            l.OrderItemId == "order-item-2" && l.ProductId == "product-2" && l.QuantityRequested == 5);
        allocation.Lines.Should().OnlyContain(l => l.Status == AllocationLineStatus.Pending);
    }

    [Test]
    public void CreateForOrder_should_raise_AllocationInitiatedDomainEvent()
    {
        var orderId = "order-1";
        var lineInputs = new[] { ("order-item-1", "product-1", 3) };

        var allocation = Allocation.CreateForOrder(orderId, lineInputs);

        allocation.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<AllocationInitiatedDomainEvent>();
        var ev = (AllocationInitiatedDomainEvent)allocation.DomainEvents.Single();
        ev.AllocationId.Should().Be(allocation.Id);
        ev.Status.Should().Be(AllocationStatus.Pending);
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public void CreateForOrder_should_throw_when_order_id_is_invalid(string? orderId)
    {
        var lineInputs = new[] { ("oi-1", "p-1", 1) };

        var act = () => Allocation.CreateForOrder(orderId!, lineInputs);

        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void CreateForOrder_should_throw_when_order_id_exceeds_max_length()
    {
        var orderId = new string('o', AllocationTableConsts.OrderIdMaxLength + 1);
        var lineInputs = new[] { ("oi-1", "p-1", 1) };

        var act = () => Allocation.CreateForOrder(orderId, lineInputs);

        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void CreateForOrder_should_throw_when_lines_are_empty()
    {
        var act = () => Allocation.CreateForOrder("order-1", Array.Empty<(string, string, int)>());

        act.Should().Throw<ArgumentException>();
    }

    private static Allocation CreateFullyAllocatedAllocation()
    {
        var allocation = Allocation.CreateForOrder("order-1", [("oi-1", "p-1", 2)]);
        allocation.ClearDomainEvents();
        var line = allocation.Lines.Single();
        var batch = new Batch("p-1", 10, 10m);
        typeof(Entity).GetProperty(nameof(Entity.Id))!.SetValue(batch, "batch-1");
        line.RecordBatchAllocation(batch.AllocateForOrder("oi-1", 2, DateTimeOffset.UtcNow));
        line.MarkAsAllocated();
        allocation.TryMarkAsAllocated();
        allocation.ClearDomainEvents();
        return allocation;
    }
}
