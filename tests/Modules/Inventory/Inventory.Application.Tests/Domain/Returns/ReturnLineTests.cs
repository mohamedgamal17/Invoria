using FluentAssertions;
using Invoria.Inventory.Domain.Returns;

namespace Invoria.Inventory.Application.Tests.Domain.Returns;

[TestFixture]
public class ReturnLineTests
{
    [Test]
    public void Should_create_line_with_valid_values()
    {
        var line = new ReturnLine(
            "line-1",
            "return-1",
            "order-item-1",
            "product-1",
            5);

        line.Id.Should().Be("line-1");
        line.ReturnId.Should().Be("return-1");
        line.OrderItemId.Should().Be("order-item-1");
        line.ProductId.Should().Be("product-1");
        line.Quantity.Should().Be(5);
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public void Should_throw_when_order_item_id_is_invalid(string? orderItemId)
    {
        var act = () => new ReturnLine("line-1", "return-1", orderItemId!, "p-1", 1);

        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Should_throw_when_order_item_id_exceeds_max_length()
    {
        var orderItemId = new string('o', ReturnLineTableConsts.OrderItemIdMaxLength + 1);

        var act = () => new ReturnLine("line-1", "return-1", orderItemId, "p-1", 1);

        act.Should().Throw<ArgumentException>();
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public void Should_throw_when_product_id_is_invalid(string? productId)
    {
        var act = () => new ReturnLine("line-1", "return-1", "oi-1", productId!, 1);

        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Should_throw_when_product_id_exceeds_max_length()
    {
        var productId = new string('p', ReturnLineTableConsts.ProductIdMaxLength + 1);

        var act = () => new ReturnLine("line-1", "return-1", "oi-1", productId, 1);

        act.Should().Throw<ArgumentException>();
    }

    [Test]
    [TestCase(0)]
    [TestCase(-1)]
    public void Should_throw_when_quantity_is_not_positive(int quantity)
    {
        var act = () => new ReturnLine("line-1", "return-1", "oi-1", "p-1", quantity);

        act.Should().Throw<ArgumentException>();
    }
}
