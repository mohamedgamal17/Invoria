using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Ordering.Contracts.Orders;
using Invoria.Ordering.Domain.Orders;
using Invoria.Ordering.Domain.Orders.Events;

namespace Invoria.Ordering.Application.Tests.Domain.Orders;

[TestFixture]
public class OrderReopenDomainTests
{
    [Test]
    public void Reopen_when_processing_sets_revision_and_raises_release_domain_event()
    {
        var order = new Order("N-R2", "cust");
        typeof(Entity<string>).GetProperty(nameof(Entity<string>.Id))!.SetValue(order, "order-reopen");
        order.UpdateItems(new List<OrderItem> { new("p1", 2, 10m) });
        order.Accept();
        var item = order.Items[0];
        typeof(Entity<string>).GetProperty(nameof(Entity<string>.Id))!.SetValue(item, "line-alloc");
        order.ClearDomainEvents();

        order.Reopen();

        order.Status.Should().Be(OrderStatus.Revision);
        order.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<OrderReopenReleaseRequestedDomainEvent>();
    }

    [Test]
    public void Reopen_throws_when_order_status_is_not_processing()
    {
        var order = new Order("N-R8", "cust");
        order.UpdateItems(new List<OrderItem> { new("p1", 1, 10m) });

        var act = () => order.Reopen();

        act.Should().Throw<InvalidOperationException>();
    }

}
