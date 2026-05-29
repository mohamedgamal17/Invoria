using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.Inventory.Domain.Allocations;
using Invoria.Inventory.Domain.Batches;
using Invoria.Inventory.Domain.Fulfillments;
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
