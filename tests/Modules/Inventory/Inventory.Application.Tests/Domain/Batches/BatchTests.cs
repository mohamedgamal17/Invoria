using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Inventory.Domain.Batches;
using Invoria.Inventory.Application.Tests.Assertions;

namespace Invoria.Inventory.Application.Tests.Domain.Batches;

[TestFixture]
public class BatchTests
{
    [Test]
    public void Should_create_batch_with_valid_values()
    {
        var productId = Guid.NewGuid().ToString();
        const int quantity = 10;
        const decimal purchasePrice = 25.5m;
        var purchaseOrderItemId = Guid.NewGuid().ToString();

        var batch = new Batch(productId, quantity, purchasePrice, purchaseOrderItemId);

        batch.AssertBatch(productId, quantity, purchasePrice, purchaseOrderItemId: purchaseOrderItemId);
        batch.State.Should().Be(BatchState.Active);
    }

    [Test]
    [TestCase("")]
    [TestCase(" ")]
    public void Should_throw_when_purchase_order_item_id_is_invalid(string purchaseOrderItemId)
    {
        var exception = Assert.Catch<ArgumentException>(() => new Batch("product-1", 1, 10m, purchaseOrderItemId));

        Assert.That(exception, Is.Not.Null);
    }

    [Test]
    public void Should_create_batch_as_depleted_when_quantity_is_zero()
    {
        var batch = new Batch(Guid.NewGuid().ToString(), 0, 10m);

        batch.Quantity.Should().Be(0);
        batch.State.Should().Be(BatchState.Depleted);
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public void Should_throw_when_product_id_is_invalid(string? productId)
    {
        var exception = Assert.Catch<ArgumentException>(() => new Batch(productId!, 1,  10m));

        Assert.That(exception, Is.Not.Null);
    }

    [Test]
    public void Should_throw_when_product_id_exceeds_max_length()
    {
        var productId = new string('a', BatchTableConsts.ProductIdMaxLength + 1);

        var exception = Assert.Catch<ArgumentException>(() => new Batch(productId, 1,  10m));

        Assert.That(exception, Is.Not.Null);
    }

    [Test]
    public void Should_throw_when_quantity_is_negative()
    {
        var exception = Assert.Throws<ArgumentException>(() => new Batch("product-1", -1,  10m));

        Assert.That(exception, Is.Not.Null);
    }

    [Test]
    public void Should_update_quantity_with_valid_value()
    {
        var batch = new Batch("product-1", 10, 10m);

        batch.UpdateQuantity(0);

        batch.Quantity.Should().Be(0);
        batch.PurchasePrice.Should().Be(10m);
        batch.State.Should().Be(BatchState.Depleted);
    }

    [Test]
    public void Should_transition_from_depleted_to_active_when_quantity_becomes_positive()
    {
        var batch = new Batch("product-1", 0, 10m);

        batch.UpdateQuantity(5);

        batch.Quantity.Should().Be(5);
        batch.State.Should().Be(BatchState.Active);
    }

    [Test]
    public void Should_disable_batch_when_active()
    {
        var batch = new Batch("product-1", 1, 10m);

        batch.Disable();

        batch.State.Should().Be(BatchState.Disabled);
    }

    [Test]
    public void Should_throw_when_disabling_batch_that_is_not_active()
    {
        var batch = new Batch("product-1", 0, 10m);

        var exception = Assert.Throws<InvalidOperationException>(() => batch.Disable());

        Assert.That(exception, Is.Not.Null);
    }

    [Test]
    public void Should_enable_batch_when_disabled_and_has_positive_quantity()
    {
        var batch = new Batch("product-1", 1, 10m);
        batch.Disable();

        batch.Enable();

        batch.State.Should().Be(BatchState.Active);
    }

    [Test]
    public void Should_throw_when_enabling_batch_with_zero_quantity()
    {
        var batch = new Batch("product-1", 1, 10m);
        batch.Disable();
        batch.UpdateQuantity(0);

        var exception = Assert.Throws<InvalidOperationException>(() => batch.Enable());

        Assert.That(exception, Is.Not.Null);
    }

    [Test]
    public void Should_not_change_state_when_updating_quantity_while_disabled()
    {
        var batch = new Batch("product-1", 1, 10m);
        batch.Disable();

        batch.UpdateQuantity(0);

        batch.State.Should().Be(BatchState.Disabled);
        batch.Quantity.Should().Be(0);
    }

    [Test]
    public void Should_throw_when_update_quantity_is_negative()
    {
        var batch = new Batch("product-1", 10, 10m);

        var exception = Assert.Throws<ArgumentException>(() => batch.UpdateQuantity(-1));

        Assert.That(exception, Is.Not.Null);
    }

    [Test]
    public void Should_update_purchase_price_with_valid_value()
    {
        var batch = new Batch("product-1", 10, 10m);

        batch.UpdatePurchasePrice(20m);

        batch.Quantity.Should().Be(10);
        batch.PurchasePrice.Should().Be(20m);
    }

    [Test]
    [TestCase(0)]
    [TestCase(-1)]
    public void Should_throw_when_update_purchase_price_is_less_than_or_equal_to_zero(decimal purchasePrice)
    {
        var batch = new Batch("product-1", 10, 10m);

        var exception = Assert.Throws<ArgumentException>(() => batch.UpdatePurchasePrice(purchasePrice));

        Assert.That(exception, Is.Not.Null);
    }


    [Test]
    [TestCase(0)]
    [TestCase(-1)]
    public void Should_throw_when_purchase_price_is_less_than_or_equal_to_zero(decimal purchasePrice)
    {
        var exception = Assert.Throws<ArgumentException>(() => new Batch("product-1", 1, purchasePrice));

        Assert.That(exception, Is.Not.Null);
    }

    [Test]
    public void AllocateForOrder_reserves_quantity_and_adds_allocation_when_active()
    {
        var batch = new Batch("product-1", 10, 10m);
        SetEntityId(batch, "batch-alloc-1");
        var at = new DateTimeOffset(2026, 4, 3, 12, 0, 0, TimeSpan.Zero);

        var allocation = batch.AllocateForOrder("order-item-1", 4, at);

        batch.Quantity.Should().Be(6);
        batch.ReservedQuantity.Should().Be(4);
        allocation.BatchId.Should().Be("batch-alloc-1");
        allocation.OrderItemId.Should().Be("order-item-1");
        allocation.QuantityAllocated.Should().Be(4);
        allocation.AllocatedAt.Should().Be(at);
    }

    [Test]
    public void AllocateForOrder_throws_when_batch_is_not_active()
    {
        var batch = new Batch("product-1", 0, 10m);
        SetEntityId(batch, "batch-depl");

        var act = () => batch.AllocateForOrder("oi-1", 1, DateTimeOffset.UtcNow);

        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void AllocateForOrder_throws_when_amount_exceeds_available()
    {
        var batch = new Batch("product-1", 5, 10m);
        SetEntityId(batch, "batch-1");
        batch.AllocateForOrder("oi-1", 3, DateTimeOffset.UtcNow);

        var act = () => batch.AllocateForOrder("oi-2", 3, DateTimeOffset.UtcNow);

        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    [TestCase(0)]
    [TestCase(-1)]
    public void AllocateForOrder_throws_when_amount_not_positive(int amount)
    {
        var batch = new Batch("product-1", 5, 10m);
        SetEntityId(batch, "batch-1");

        var act = () => batch.AllocateForOrder("oi-1", amount, DateTimeOffset.UtcNow);

        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void ReleaseReservedForDispatch_reduces_ReservedQuantity_only()
    {
        var batch = new Batch("product-1", 10, 10m);
        SetEntityId(batch, "batch-r1");
        batch.AllocateForOrder("oi-1", 4, DateTimeOffset.UtcNow);

        batch.ReleaseReservedForDispatch(4);

        batch.Quantity.Should().Be(6);
        batch.ReservedQuantity.Should().Be(0);
    }

    [Test]
    public void ReleaseReservedForDispatch_supports_partial_release()
    {
        var batch = new Batch("product-1", 10, 10m);
        SetEntityId(batch, "batch-r2");
        batch.AllocateForOrder("oi-1", 5, DateTimeOffset.UtcNow);

        batch.ReleaseReservedForDispatch(2);

        batch.Quantity.Should().Be(5);
        batch.ReservedQuantity.Should().Be(3);
    }

    [Test]
    public void ReleaseReservedForDispatch_throws_when_amount_exceeds_reserved()
    {
        var batch = new Batch("product-1", 10, 10m);
        SetEntityId(batch, "batch-r3");
        batch.AllocateForOrder("oi-1", 3, DateTimeOffset.UtcNow);

        var act = () => batch.ReleaseReservedForDispatch(4);

        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    [TestCase(0)]
    [TestCase(-1)]
    public void ReleaseReservedForDispatch_throws_when_amount_not_positive(int amount)
    {
        var batch = new Batch("product-1", 10, 10m);
        SetEntityId(batch, "batch-r4");
        batch.AllocateForOrder("oi-1", 1, DateTimeOffset.UtcNow);

        var act = () => batch.ReleaseReservedForDispatch(amount);

        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void RestoreAllocatedQuantity_returns_stock_and_reduces_reserved()
    {
        var batch = new Batch("product-1", 10, 10m);
        SetEntityId(batch, "batch-restore-1");
        batch.AllocateForOrder("oi-1", 4, DateTimeOffset.UtcNow);

        batch.RestoreAllocatedQuantity(4);

        batch.Quantity.Should().Be(10);
        batch.ReservedQuantity.Should().Be(0);
        batch.State.Should().Be(BatchState.Active);
    }

    [Test]
    public void RestoreAllocatedQuantity_reactivates_depleted_batch_when_quantity_positive()
    {
        var batch = new Batch("product-1", 4, 10m);
        SetEntityId(batch, "batch-restore-depl");
        batch.AllocateForOrder("oi-1", 4, DateTimeOffset.UtcNow);
        batch.State.Should().Be(BatchState.Depleted);

        batch.RestoreAllocatedQuantity(4);

        batch.Quantity.Should().Be(4);
        batch.ReservedQuantity.Should().Be(0);
        batch.State.Should().Be(BatchState.Active);
    }

    [Test]
    public void RestoreAllocatedQuantity_throws_when_amount_exceeds_reserved()
    {
        var batch = new Batch("product-1", 10, 10m);
        SetEntityId(batch, "batch-restore-bad");
        batch.AllocateForOrder("oi-1", 2, DateTimeOffset.UtcNow);

        var act = () => batch.RestoreAllocatedQuantity(3);

        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void RestoreAllocatedQuantity_throws_on_disabled_batch()
    {
        var batch = new Batch("product-1", 10, 10m);
        SetEntityId(batch, "batch-restore-dis");
        batch.AllocateForOrder("oi-1", 2, DateTimeOffset.UtcNow);
        batch.Disable();

        var act = () => batch.RestoreAllocatedQuantity(2);

        act.Should().Throw<InvalidOperationException>();
    }

    private static void SetEntityId(Batch batch, string id)
    {
        typeof(Entity).GetProperty(nameof(Entity.Id))!.SetValue(batch, id);
    }
}
