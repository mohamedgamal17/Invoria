using FluentAssertions;
using Invoria.Inventory.Domain.Batches;

namespace Invoria.Inventory.Application.Tests.Domain.Batches;

[TestFixture]
public class BatchAllocationTests
{
    [Test]
    public void Should_create_allocation_with_valid_values()
    {
        var batchId = "batch-1";
        var orderItemId = "order-item-1";
        const int quantity = 5;
        var allocatedAt = new DateTimeOffset(2026, 4, 1, 12, 0, 0, TimeSpan.Zero);

        var allocation = new BatchAllocation(batchId, orderItemId, quantity, allocatedAt);

        allocation.BatchId.Should().Be(batchId);
        allocation.OrderItemId.Should().Be(orderItemId);
        allocation.QuantityAllocated.Should().Be(quantity);
        allocation.AllocatedAt.Should().Be(allocatedAt);
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public void Should_throw_when_batch_id_is_invalid(string? batchId)
    {
        var exception = Assert.Catch<ArgumentException>(() =>
            new BatchAllocation(batchId!, "oi-1", 1, DateTimeOffset.UtcNow));

        Assert.That(exception, Is.Not.Null);
    }

    [Test]
    public void Should_throw_when_batch_id_exceeds_max_length()
    {
        var batchId = new string('b', BatchAllocationTableConsts.BatchIdMaxLength + 1);

        var exception = Assert.Catch<ArgumentException>(() =>
            new BatchAllocation(batchId, "oi-1", 1, DateTimeOffset.UtcNow));

        Assert.That(exception, Is.Not.Null);
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public void Should_throw_when_order_item_id_is_invalid(string? orderItemId)
    {
        var exception = Assert.Catch<ArgumentException>(() =>
            new BatchAllocation("batch-1", orderItemId!, 1, DateTimeOffset.UtcNow));

        Assert.That(exception, Is.Not.Null);
    }

    [Test]
    public void Should_throw_when_order_item_id_exceeds_max_length()
    {
        var orderItemId = new string('o', BatchAllocationTableConsts.OrderItemIdMaxLength + 1);

        var exception = Assert.Catch<ArgumentException>(() =>
            new BatchAllocation("batch-1", orderItemId, 1, DateTimeOffset.UtcNow));

        Assert.That(exception, Is.Not.Null);
    }

    [Test]
    [TestCase(0)]
    [TestCase(-1)]
    public void Should_throw_when_quantity_allocated_is_not_positive(int quantity)
    {
        var exception = Assert.Catch<ArgumentException>(() =>
            new BatchAllocation("batch-1", "oi-1", quantity, DateTimeOffset.UtcNow));

        Assert.That(exception, Is.Not.Null);
    }
}
