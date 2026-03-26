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
    public void Should_update_batch_with_valid_values()
    {
        var batch = new Batch("product-1", 10, 10m);

        batch.Update(0, 20m);

        batch.Quantity.Should().Be(0);
        batch.PurchasePrice.Should().Be(20m);
    }

    [Test]
    public void Should_throw_when_update_quantity_is_negative()
    {
        var batch = new Batch("product-1", 10, 10m);

        var exception = Assert.Throws<ArgumentException>(() => batch.Update(-1, 20m));

        Assert.That(exception, Is.Not.Null);
    }

    [Test]
    [TestCase(0)]
    [TestCase(-1)]
    public void Should_throw_when_update_purchase_price_is_less_than_or_equal_to_zero(decimal purchasePrice)
    {
        var batch = new Batch("product-1", 10, 10m);

        var exception = Assert.Throws<ArgumentException>(() => batch.Update(0, purchasePrice));

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
