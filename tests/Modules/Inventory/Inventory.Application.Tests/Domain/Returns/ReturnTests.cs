using FluentAssertions;
using Invoria.Inventory.Domain.Returns;

namespace Invoria.Inventory.Application.Tests.Domain.Returns;

[TestFixture]
public class ReturnTests
{
    [Test]
    public void Create_should_create_return_with_order_id_and_lines()
    {
        var orderId = "order-1";
        var returnLines = new[]
        {
            ReturnLine.Create("order-item-1", "product-1", 3),
            ReturnLine.Create("order-item-2", "product-2", 5)
        };

        var @return = TestReturn.Create(orderId, returnLines);

        @return.OrderId.Should().Be(orderId);
        @return.ReturnLines.Should().HaveCount(2);
        @return.ReturnLines.Should().ContainSingle(l =>
            l.OrderItemId == "order-item-1" && l.ProductId == "product-1" && l.Quantity == 3);
        @return.ReturnLines.Should().ContainSingle(l =>
            l.OrderItemId == "order-item-2" && l.ProductId == "product-2" && l.Quantity == 5);
        @return.ReturnLines.Should().OnlyContain(l => l.ReturnId == @return.Id);
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public void Create_should_throw_when_order_id_is_invalid(string? orderId)
    {
        var returnLines = new[] { ReturnLine.Create("oi-1", "p-1", 1) };

        var act = () => TestReturn.Create(orderId!, returnLines);

        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Create_should_throw_when_order_id_exceeds_max_length()
    {
        var orderId = new string('o', ReturnTableConsts.OrderIdMaxLength + 1);
        var returnLines = new[] { ReturnLine.Create("oi-1", "p-1", 1) };

        var act = () => TestReturn.Create(orderId, returnLines);

        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Create_should_throw_when_lines_are_empty()
    {
        var act = () => TestReturn.Create("order-1", Array.Empty<ReturnLine>());

        act.Should().Throw<ArgumentException>();
    }

    private sealed class TestReturn : Return
    {
        public static TestReturn Create(string orderId, IEnumerable<ReturnLine> returnLines) =>
            new(orderId, returnLines);

        private TestReturn(string orderId, IEnumerable<ReturnLine> returnLines)
            : base(orderId, returnLines)
        {
        }
    }
}
