using Invoria.Inventory.Domain.Batches;

namespace Invoria.Inventory.Application.Tests.Domain.Batches;

[TestFixture]
public class BatchTests
{
    [Test]
    public void Should_create_batch_with_valid_values()
    {
        var productId = Guid.NewGuid().ToString();
        const int quantity = 10;
        const int reservedQuantity = 2;
        const decimal purchasePrice = 25.5m;

        var batch = new Batch(productId, quantity, reservedQuantity, purchasePrice);

        Assert.That(batch.ProductId, Is.EqualTo(productId));
        Assert.That(batch.Quantity, Is.EqualTo(quantity));
        Assert.That(batch.ReservedQuantity, Is.EqualTo(reservedQuantity));
        Assert.That(batch.PurchasePrice, Is.EqualTo(purchasePrice));
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public void Should_throw_when_product_id_is_invalid(string? productId)
    {
        var exception = Assert.Catch<ArgumentException>(() => new Batch(productId!, 1, 0, 10m));

        Assert.That(exception, Is.Not.Null);
    }

    [Test]
    public void Should_throw_when_product_id_exceeds_max_length()
    {
        var productId = new string('a', BatchTableConsts.ProductIdMaxLength + 1);

        var exception = Assert.Catch<ArgumentException>(() => new Batch(productId, 1, 0, 10m));

        Assert.That(exception, Is.Not.Null);
    }

    [Test]
    public void Should_throw_when_quantity_is_negative()
    {
        var exception = Assert.Throws<ArgumentException>(() => new Batch("product-1", -1, 0, 10m));

        Assert.That(exception, Is.Not.Null);
    }

    [Test]
    public void Should_throw_when_reserved_quantity_is_negative()
    {
        var exception = Assert.Throws<ArgumentException>(() => new Batch("product-1", 1, -1, 10m));

        Assert.That(exception, Is.Not.Null);
    }

    [Test]
    [TestCase(0)]
    [TestCase(-1)]
    public void Should_throw_when_purchase_price_is_less_than_or_equal_to_zero(decimal purchasePrice)
    {
        var exception = Assert.Throws<ArgumentException>(() => new Batch("product-1", 1, 0, purchasePrice));

        Assert.That(exception, Is.Not.Null);
    }
}
