using FluentAssertions;
using Invoria.Inventory.Domain.Allocations;

namespace Invoria.Inventory.Application.Tests.Domain.Allocations;

[TestFixture]
public class AllocationTests
{
    [Test]
    public void CreateForOrder_should_create_allocation_with_pending_status_and_lines()
    {
        var orderId = "order-1";
        var lineInputs = new[]
        {
            ("order-item-1", "product-1", 3),
            ("order-item-2", "product-2", 5)
        };

        var allocation = Allocation.CreateForOrder(orderId, lineInputs);

        allocation.OrderId.Should().Be(orderId);
        allocation.Status.Should().Be(AllocationStatus.Pending);
        allocation.Lines.Should().HaveCount(2);
        allocation.Lines.Should().ContainSingle(l =>
            l.OrderItemId == "order-item-1" && l.ProductId == "product-1" && l.QuantityRequested == 3);
        allocation.Lines.Should().ContainSingle(l =>
            l.OrderItemId == "order-item-2" && l.ProductId == "product-2" && l.QuantityRequested == 5);
        allocation.Lines.Should().OnlyContain(l => l.Status == AllocationLineStatus.Pending);
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public void CreateForOrder_should_throw_when_order_id_is_invalid(string? orderId)
    {
        var lineInputs = new[] { ("oi-1", "p-1", 1) };

        var act = () => Allocation.CreateForOrder(orderId!, lineInputs);

        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void CreateForOrder_should_throw_when_order_id_exceeds_max_length()
    {
        var orderId = new string('o', AllocationTableConsts.OrderIdMaxLength + 1);
        var lineInputs = new[] { ("oi-1", "p-1", 1) };

        var act = () => Allocation.CreateForOrder(orderId, lineInputs);

        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void CreateForOrder_should_throw_when_lines_are_empty()
    {
        var act = () => Allocation.CreateForOrder("order-1", Array.Empty<(string, string, int)>());

        act.Should().Throw<ArgumentException>();
    }
}
