using FluentAssertions;
using Invoria.Ordering.Contracts.Orders.Enums;
using Invoria.Ordering.Domain.Orders;

namespace Invoria.Ordering.Application.Tests.Domain.Orders;

[TestFixture]
public class OrderReviseDomainTests
{
    [Test]
    public void Revise_when_pending_sets_processing()
    {
        var order = new Order("N-R9", "cust");
        order.UpdateItems(new List<OrderItem> { new("p1", 1, 10m) });

        order.Revise();

        order.Status.Should().Be(OrderStatus.Processing);
    }

    [Test]
    public void Revise_throws_when_not_pending_or_revision()
    {
        var order = new Order("N-R8", "cust");
        order.UpdateItems(new List<OrderItem> { new("p1", 1, 10m) });
        order.Revise();

        var act = () => order.Revise();

        act.Should().Throw<InvalidOperationException>();
    }
}
