using Autofac;
using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.Ordering.Application.Orders.Commands.AcceptOrder;
using Invoria.Ordering.Application.Orders.Commands.CompleteOrder;
using Invoria.Ordering.Application.Orders.Commands.RecordOrderAllocationSucceeded;
using Invoria.Ordering.Domain.Orders;
using Invoria.Ordering.Infrastructure.EntityFramework;
using Invoria.Ordering.Tests.Fakes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Rebus.Bus;

namespace Invoria.Ordering.Application.Tests.Integration.Commands;

[TestFixture]
public class CompleteOrderCommandHandlerTests : OrderTestFixture
{
    protected override async Task BeforeAnyTestRunAsync()
    {
        await ClearOrdersAsync();
        var busMock = ServiceProvider.GetRequiredService<Mock<IBus>>();
        busMock.Invocations.Clear();
    }

    private async Task ClearOrdersAsync()
    {
        var db = Scope.Resolve<OrderingDbContext>();
        var orders = await db.Set<Order>().ToListAsync();
        db.RemoveRange(orders);
        await db.SaveChangesAsync();
    }

    private async Task SetOrderStatusAsync(string orderId, OrderStatus status)
    {
        var db = Scope.Resolve<OrderingDbContext>();
        var rows = await db.Set<Order>()
            .Where(o => o.Id == orderId)
            .ExecuteUpdateAsync(s => s.SetProperty(o => o.Status, status));

        rows.Should().Be(1, $"order id {orderId} should exist for status update");
    }

    private async Task<OrderStatus> GetOrderStatusFromDbAsync(string orderId)
    {
        var db = Scope.Resolve<OrderingDbContext>();
        return await db.Set<Order>()
            .Where(o => o.Id == orderId)
            .Select(o => o.Status)
            .SingleAsync();
    }

    [Test]
    public async Task Should_complete_order_when_accepted_and_dispatched()
    {
        var order = (await OrderTestData.PersistRandomOrdersAsync(OrderRepository, 1)).Single();
        await Mediator.Send(new AcceptOrderCommand(order.Id));
        await Mediator.Send(new RecordOrderAllocationSucceededCommand
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId
        });
        await OrderFulfillmentTestTransitions.DispatchAndShipAsync(OrderRepository, order.Id);

        var command = new CompleteOrderCommand(order.Id);

        var result = await Mediator.Send(command);

        result.ShouldBeSuccess();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(order.Id);

        var status = await GetOrderStatusFromDbAsync(order.Id);
        status.Should().Be(OrderStatus.Completed);
    }

    [Test]
    public async Task Should_fail_when_accepted_but_not_dispatched()
    {
        var order = (await OrderTestData.PersistRandomOrdersAsync(OrderRepository, 1)).Single();
        await Mediator.Send(new AcceptOrderCommand(order.Id));
        await Mediator.Send(new RecordOrderAllocationSucceededCommand
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId
        });

        var result = await Mediator.Send(new CompleteOrderCommand(order.Id));

        result.ShouldBeFailure(typeof(BusinessLogicException));
    }

    [Test]
    [TestCase(OrderStatus.Pending)]
    [TestCase(OrderStatus.Reopened)]
    [TestCase(OrderStatus.Completed)]
    [TestCase(OrderStatus.Cancelled)]
    public async Task Should_fail_when_order_is_not_accepted(OrderStatus status)
    {
        var order = (await OrderTestData.PersistRandomOrdersAsync(OrderRepository, 1)).Single();
        await SetOrderStatusAsync(order.Id, status);

        var command = new CompleteOrderCommand(order.Id);

        var result = await Mediator.Send(command);

        result.ShouldBeFailure(typeof(BusinessLogicException));
    }

    [Test]
    public async Task Should_fail_when_order_not_found()
    {
        var command = new CompleteOrderCommand(Guid.NewGuid().ToString());

        var result = await Mediator.Send(command);

        result.ShouldBeFailure(typeof(NotFoundException));
    }
}
