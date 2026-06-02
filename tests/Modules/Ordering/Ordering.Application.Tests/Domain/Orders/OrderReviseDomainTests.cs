using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Ordering.Contracts.Orders;
using Invoria.Ordering.Domain.Orders;

namespace Invoria.Ordering.Application.Tests.Domain.Orders;

[TestFixture]
public class OrderReviseDomainTests
{
    [Test]
    public void Revise_when_processing_sets_revision_without_domain_events()
    {
        var order = new Order("N-R2", "cust");
        typeof(Entity<string>).GetProperty(nameof(Entity<string>.Id))!.SetValue(order, "order-revise");
        order.UpdateItems(new List<OrderItem> { new("p1", 2, 10m) });
        order.Accept();
        order.ClearDomainEvents();

        order.Revise();

        order.Status.Should().Be(OrderStatus.Revision);
        order.DomainEvents.Should().BeEmpty();
    }

    [Test]
    public void Revise_throws_when_order_status_is_not_processing()
    {
        var order = new Order("N-R8", "cust");
        order.UpdateItems(new List<OrderItem> { new("p1", 1, 10m) });

        var act = () => order.Revise();

        act.Should().Throw<InvalidOperationException>();
    }
}
