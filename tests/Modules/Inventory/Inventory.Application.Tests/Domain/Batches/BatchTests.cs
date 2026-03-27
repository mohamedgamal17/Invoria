using FluentAssertions;
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

        var batch = new Batch(productId, quantity,  purchasePrice);

        batch.AssertBatch(productId, quantity, purchasePrice);
        batch.State.Should().Be(BatchState.Active);
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
}
