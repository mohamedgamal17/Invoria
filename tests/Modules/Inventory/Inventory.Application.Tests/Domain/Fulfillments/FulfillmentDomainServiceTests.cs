using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.Inventory.Domain.Allocations;
using Invoria.Inventory.Domain.Batches;
using Invoria.Inventory.Domain.Fulfillments;
using Invoria.Inventory.Domain.Fulfillments.Events;
using Invoria.Inventory.Domain.Fulfillments.Services;

namespace Invoria.Inventory.Application.Tests.Domain.Fulfillments;

[TestFixture]
public class FulfillmentDomainServiceTests
{
    private readonly FulfillmentDomainService _sut = new();

    [Test]
    public void CreateFulfillment_succeeds_when_allocation_is_allocated()
    {
        var allocation = CreateFullyAllocatedAllocation("order-1");

        var result = _sut.CreateFulfillment(allocation);

        result.IsSuccess.Should().BeTrue();
        result.Value!.OrderId.Should().Be("order-1");
        result.Value.AllocationId.Should().Be(allocation.Id);
        result.Value.Status.Should().Be(FulfillmentStatus.Pending);
        result.Value.Items.Should().HaveCount(1);
    }

    [Test]
    public void CreateFulfillment_fails_when_allocation_is_not_allocated()
    {
        var allocation = Allocation.CreateForOrder("order-1", [("oi-1", "p-1", 2)]);

        var result = _sut.CreateFulfillment(allocation);

        result.IsFailure.Should().BeTrue();
        result.Exception.Should().BeOfType<BusinessLogicException>()
            .Which.Message.Should().Contain("Allocated");
    }

    [Test]
    public void Dispatch_succeeds_and_consumes_reserved_stock_marks_allocation_and_completes_fulfillment()
    {
        var allocation = CreateFullyAllocatedAllocation("order-1");
        var batch = CreateBatchWithReservedStock(allocation);
        var fulfillment = Fulfillment.CreateFromAllocation(allocation);
        fulfillment.ClearDomainEvents();
        fulfillment.RequestDispatch();
        fulfillment.ClearDomainEvents();

        var result = _sut.Dispatch(
            fulfillment,
            allocation,
            new Dictionary<string, Batch> { [batch.Id!] = batch });

        result.IsSuccess.Should().BeTrue();
        batch.ReservedQuantity.Should().Be(0);
        batch.Quantity.Should().Be(8);
        allocation.Status.Should().Be(AllocationStatus.Allocated);
        fulfillment.Status.Should().Be(FulfillmentStatus.Completed);
        fulfillment.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<FulfillmentCompletedDomainEvent>();
    }

    private static Batch CreateBatchWithReservedStock(Allocation allocation)
    {
        var line = allocation.Lines.Single();
        var batchAllocation = line.BatchAllocations.Single();
        var batch = new Batch("p-1", 10, 10m);
        typeof(Entity).GetProperty(nameof(Entity.Id))!.SetValue(batch, batchAllocation.BatchId);
        batch.AllocateForOrder(line.OrderItemId, batchAllocation.QuantityAllocated, DateTimeOffset.UtcNow);
        return batch;
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
