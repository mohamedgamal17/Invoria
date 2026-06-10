using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Ordering.Contracts.Orders.Enums;
using Invoria.Ordering.Domain.Orders;

namespace Invoria.Ordering.Application.Tests.Domain.Orders;

[TestFixture]
public class OrderRecordReturnDomainTests
{
    private static void SetEntityId(Entity<string> entity, string id)
    {
        typeof(Entity<string>).GetProperty(nameof(Entity<string>.Id))!.SetValue(entity, id);
    }

    [Test]
    public void RecordReturn_sets_return_id_when_completed()
    {
        var order = new Order("TEST-1", Guid.NewGuid().ToString());
        order.UpdateItems([new OrderItem("p1", 2, 10m)]);
        SetEntityId(order, "order-1");
        order.Accept();
        order.Complete([]);

        order.RecordReturn("return-1");

        order.ReturnId.Should().Be("return-1");
        order.Status.Should().Be(OrderStatus.Completed);
    }

    [Test]
    public void RecordReturn_throws_when_not_completed()
    {
        var order = new Order("TEST-2", Guid.NewGuid().ToString());
        order.UpdateItems([new OrderItem("p1", 1, 10m)]);
        order.Accept();

        var act = () => order.RecordReturn("return-1");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Completed*");
        order.ReturnId.Should().BeNull();
    }
}
