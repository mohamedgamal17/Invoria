using FluentAssertions;
using Invoria.Ordering.Contracts.Orders.Enums;
using Invoria.Ordering.Domain.Orders;
using Invoria.Ordering.Domain.Orders.Events;

namespace Invoria.Ordering.Application.Tests.Domain.Orders;

[TestFixture]
public class OrderCreatedDomainTests
{
    [Test]
    public void Create_raises_OrderCreatedDomainEvent_with_order_instance()
    {
        const string orderNumber = "ON-CREATE-1";
        const string customerId = "cust-create-1";

        var order = Order.Create(
            orderNumber,
            customerId,
            OrderPaymentType.Debt,
            [new OrderItem("p1", 2, 10m)]);

        order.Id.Should().NotBeNullOrWhiteSpace();
        order.OrderNumber.Should().Be(orderNumber);
        order.CustomerId.Should().Be(customerId);

        order.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<OrderCreatedDomainEvent>();

        var domainEvent = (OrderCreatedDomainEvent)order.DomainEvents.Single();
        domainEvent.Order.Should().BeSameAs(order);
        domainEvent.Order.Items.Should().ContainSingle()
            .Which.ProductId.Should().Be("p1");
    }
}
