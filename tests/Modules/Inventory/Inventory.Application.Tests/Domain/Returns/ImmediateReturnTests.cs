using FluentAssertions;
using Invoria.Inventory.Domain.Returns;

namespace Invoria.Inventory.Application.Tests.Domain.Returns;

[TestFixture]
public class ImmediateReturnTests
{
    [Test]
    public void Create_should_create_immediate_return_with_allocation_order_and_lines()
    {
        var allocationId = "allocation-1";
        var orderId = "order-1";
        var returnLines = new[]
        {
            ReturnLine.Create("order-item-1", "product-1", 3),
            ReturnLine.Create("order-item-2", "product-2", 5)
        };

        var @return = ImmediateReturn.Create(allocationId, orderId, returnLines);

        @return.Type.Should().Be(ReturnType.Immediate);
        @return.AllocationId.Should().Be(allocationId);
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
    public void Create_should_throw_when_allocation_id_is_invalid(string? allocationId)
    {
        var returnLines = new[] { ReturnLine.Create("oi-1", "p-1", 1) };

        var act = () => ImmediateReturn.Create(allocationId!, "order-1", returnLines);

        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Create_should_throw_when_allocation_id_exceeds_max_length()
    {
        var allocationId = new string('a', ImmediateReturnTableConsts.AllocationIdMaxLength + 1);
        var returnLines = new[] { ReturnLine.Create("oi-1", "p-1", 1) };

        var act = () => ImmediateReturn.Create(allocationId, "order-1", returnLines);

        act.Should().Throw<ArgumentException>();
    }
}
