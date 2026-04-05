using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Ordering.Domain.Orders;
using Invoria.Ordering.Domain.Orders.Events;

namespace Invoria.Ordering.Application.Tests.Domain.Orders;

[TestFixture]
public class OrderReopenDomainTests
{
    [Test]
    public void Reopen_when_accepted_and_fulfillment_pending_sets_on_hold_and_reopened()
    {
        var order = new Order("N-R1", "cust");
        typeof(Entity<string>).GetProperty(nameof(Entity<string>.Id))!.SetValue(order, "order-reopen-pending");
        order.UpdateItems(new List<OrderItem> { new("p1", 2, 10m) });
        order.Accept();
        order.FullfillmentStatus = FullfillmentStatus.Pending;
        order.ClearDomainEvents();

        order.Reopen();

        order.Status.Should().Be(OrderStatus.Reopened);
        order.FullfillmentStatus.Should().Be(FullfillmentStatus.OnHold);
        order.DomainEvents.Should().BeEmpty();
    }

    [Test]
    public void Reopen_when_accepted_and_allocated_sets_releasing_and_raises_release_domain_event()
    {
        var order = new Order("N-R2", "cust");
        typeof(Entity<string>).GetProperty(nameof(Entity<string>.Id))!.SetValue(order, "order-reopen-alloc");
        order.UpdateItems(new List<OrderItem> { new("p1", 2, 10m) });
        order.Accept();
        order.MarkInventoryAllocated();
        var item = order.Items[0];
        typeof(Entity<string>).GetProperty(nameof(Entity<string>.Id))!.SetValue(item, "line-alloc");
        order.ClearDomainEvents();

        order.Reopen();

        order.Status.Should().Be(OrderStatus.Accepted);
        order.FullfillmentStatus.Should().Be(FullfillmentStatus.Releasing);
        order.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<OrderReopenReleaseRequestedDomainEvent>();
        var ev = (OrderReopenReleaseRequestedDomainEvent)order.DomainEvents.Single();
        ev.OrderId.Should().Be("order-reopen-alloc");
        ev.OrderNumber.Should().Be("N-R2");
        ev.CustomerId.Should().Be("cust");
        ev.Lines.Should().ContainSingle();
        ev.Lines[0].OrderItemId.Should().Be("line-alloc");
        ev.Lines[0].ProductId.Should().Be("p1");
        ev.Lines[0].Quantity.Should().Be(2);
    }

    [Test]
    public void Reopen_throws_when_fulfillment_is_dispatched()
    {
        var order = new Order("N-R3", "cust");
        typeof(Entity<string>).GetProperty(nameof(Entity<string>.Id))!.SetValue(order, "order-reopen-disp");
        order.UpdateItems(new List<OrderItem> { new("p1", 1, 10m) });
        order.Accept();
        order.MarkInventoryAllocated();
        var item = order.Items[0];
        typeof(Entity<string>).GetProperty(nameof(Entity<string>.Id))!.SetValue(item, "line-disp");
        order.MarkDispatched();
        order.ClearDomainEvents();

        var act = () => order.Reopen();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*reopened after dispatch*");
        order.Status.Should().Be(OrderStatus.Accepted);
        order.FullfillmentStatus.Should().Be(FullfillmentStatus.Dispatched);
        order.DomainEvents.Should().BeEmpty();
    }

    [Test]
    public void CompleteReopenAfterInventoryReleased_after_reopen_from_allocated_sets_on_hold_and_reopened()
    {
        var order = new Order("N-R4", "cust");
        typeof(Entity<string>).GetProperty(nameof(Entity<string>.Id))!.SetValue(order, "order-reopen-complete");
        order.UpdateItems(new List<OrderItem> { new("p1", 1, 10m) });
        order.Accept();
        order.MarkInventoryAllocated();
        order.Reopen();
        order.ClearDomainEvents();

        order.CompleteReopenAfterInventoryReleased();

        order.Status.Should().Be(OrderStatus.Reopened);
        order.FullfillmentStatus.Should().Be(FullfillmentStatus.OnHold);
        order.DomainEvents.Should().BeEmpty();
    }

    [Test]
    public void Reopen_throws_when_fulfillment_is_allocating()
    {
        var order = new Order("N-R5", "cust");
        typeof(Entity<string>).GetProperty(nameof(Entity<string>.Id))!.SetValue(order, "order-reopen-bad");
        order.UpdateItems(new List<OrderItem> { new("p1", 1, 10m) });
        order.Accept();

        var act = () => order.Reopen();

        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void Reopen_throws_when_fulfillment_is_on_hold()
    {
        var order = new Order("N-R6", "cust");
        typeof(Entity<string>).GetProperty(nameof(Entity<string>.Id))!.SetValue(order, "order-reopen-onhold");
        order.UpdateItems(new List<OrderItem> { new("p1", 1, 10m) });
        order.Accept();
        order.FullfillmentStatus = FullfillmentStatus.OnHold;

        var act = () => order.Reopen();

        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void Reopen_throws_when_fulfillment_is_releasing()
    {
        var order = new Order("N-R7", "cust");
        typeof(Entity<string>).GetProperty(nameof(Entity<string>.Id))!.SetValue(order, "order-reopen-rel");
        order.UpdateItems(new List<OrderItem> { new("p1", 1, 10m) });
        order.Accept();
        order.MarkInventoryAllocated();
        order.Reopen();
        order.ClearDomainEvents();

        var act = () => order.Reopen();

        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void Reopen_throws_when_order_status_is_not_accepted()
    {
        var order = new Order("N-R8", "cust");
        order.UpdateItems(new List<OrderItem> { new("p1", 1, 10m) });

        var act = () => order.Reopen();

        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void CompleteReopenAfterInventoryReleased_throws_when_not_releasing()
    {
        var order = new Order("N-R9", "cust");
        order.UpdateItems(new List<OrderItem> { new("p1", 1, 10m) });
        order.Accept();
        order.MarkInventoryAllocated();

        var act = () => order.CompleteReopenAfterInventoryReleased();

        act.Should().Throw<InvalidOperationException>();
    }
}
