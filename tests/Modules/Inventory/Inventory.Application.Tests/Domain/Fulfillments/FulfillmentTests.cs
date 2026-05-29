using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Inventory.Domain.Allocations;
using Invoria.Inventory.Domain.Batches;
using Invoria.Inventory.Domain.Fulfillments;
using Invoria.Inventory.Domain.Fulfillments.Events;

namespace Invoria.Inventory.Application.Tests.Domain.Fulfillments;

[TestFixture]
public class FulfillmentTests
{
    [Test]
    public void CreateFromAllocation_should_raise_FulfillmentCreatedDomainEvent()
    {
        var allocation = CreateFullyAllocatedAllocation("order-1");

        var fulfillment = Fulfillment.CreateFromAllocation(allocation);

        fulfillment.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<FulfillmentCreatedDomainEvent>();
        var ev = (FulfillmentCreatedDomainEvent)fulfillment.DomainEvents.Single();
        ev.FulfillmentId.Should().Be(fulfillment.Id);
        ev.OrderId.Should().Be("order-1");
        ev.AllocationId.Should().Be(allocation.Id);
    }

    [Test]
    public void CreateFromAllocation_should_throw_when_allocation_is_not_allocated()
    {
        var allocation = Allocation.CreateForOrder("order-1", [("oi-1", "p-1", 2)]);

        var act = () => Fulfillment.CreateFromAllocation(allocation);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Allocated state*");
    }

    [Test]
    public void RequestDispatch_should_set_status_to_InProgress_and_raise_RequestDispatchDomainEvent()
    {
        var allocation = CreateFullyAllocatedAllocation("order-1");
        var fulfillment = Fulfillment.CreateFromAllocation(allocation);
        fulfillment.ClearDomainEvents();

        fulfillment.RequestDispatch();

        fulfillment.Status.Should().Be(FulfillmentStatus.InProgress);
        fulfillment.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<RequestDispatchDomainEvent>();
        var ev = (RequestDispatchDomainEvent)fulfillment.DomainEvents.Single();
        ev.FulfillmentId.Should().Be(fulfillment.Id);
        ev.OrderId.Should().Be("order-1");
        ev.AllocationId.Should().Be(allocation.Id);
    }

    [Test]
    public void RequestDispatch_should_throw_when_not_Pending()
    {
        var allocation = CreateFullyAllocatedAllocation("order-1");
        var fulfillment = Fulfillment.CreateFromAllocation(allocation);
        fulfillment.ClearDomainEvents();
        fulfillment.RequestDispatch();
        fulfillment.ClearDomainEvents();

        var act = () => fulfillment.RequestDispatch();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Pending*");
    }

    [Test]
    public void Complete_should_set_status_to_Completed_and_raise_FulfillmentCompletedDomainEvent()
    {
        var allocation = CreateFullyAllocatedAllocation("order-1");
        var fulfillment = Fulfillment.CreateFromAllocation(allocation);
        fulfillment.ClearDomainEvents();
        fulfillment.RequestDispatch();
        fulfillment.ClearDomainEvents();

        fulfillment.Complete();

        fulfillment.Status.Should().Be(FulfillmentStatus.Completed);
        fulfillment.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<FulfillmentCompletedDomainEvent>();
        var ev = (FulfillmentCompletedDomainEvent)fulfillment.DomainEvents.Single();
        ev.FulfillmentId.Should().Be(fulfillment.Id);
        ev.OrderId.Should().Be("order-1");
        ev.AllocationId.Should().Be(allocation.Id);
    }

    [Test]
    public void Complete_should_throw_when_not_InProgress()
    {
        var allocation = CreateFullyAllocatedAllocation("order-1");
        var fulfillment = Fulfillment.CreateFromAllocation(allocation);

        var act = () => fulfillment.Complete();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*InProgress*");
    }

    private static Allocation CreateFullyAllocatedAllocation(string orderId)
    {
        var allocation = Allocation.CreateForOrder(orderId, [("oi-1", "p-1", 2)]);
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
