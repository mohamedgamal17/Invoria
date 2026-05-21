using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.Ordering.Application.Orders.Commands.AcceptOrder;
using Invoria.Ordering.Application.Orders.Commands.DispatchOrder;
using Invoria.Ordering.Application.Orders.Commands.RecordOrderAllocationSucceeded;
using Invoria.Ordering.Application.Orders.Commands.ShipOrder;
using Invoria.Ordering.Contracts.Orders;
using Invoria.Ordering.Domain;
using Invoria.Ordering.Domain.Orders;
using Invoria.Ordering.Infrastructure.EntityFramework;
using Invoria.Ordering.Tests.Fakes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Rebus.Bus;

namespace Invoria.Ordering.Application.Tests.Integration.Commands;

[TestFixture]
public class ShipOrderCommandHandlerTests : OrderTestFixture
{
    private async Task<Order> PersistOneRandomOrderInNewScopeAsync()
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<IOrderingRepository<Order>>();
        return (await OrderTestData.PersistRandomOrdersAsync(repo, 1)).Single();
    }

    protected override async Task BeforeAnyTestRunAsync()
    {
        await ClearOrdersAsync();
        var busMock = ServiceProvider.GetRequiredService<Mock<IBus>>();
        busMock.Invocations.Clear();
    }

    private async Task ClearOrdersAsync()
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
        var orders = await db.Set<Order>().ToListAsync();
        db.RemoveRange(orders);
        await db.SaveChangesAsync();
    }

    private static async Task<OrderStatus> GetOrderStatusFromDbAsync(
        IServiceProvider serviceProvider,
        string orderId)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
        return await db.Set<Order>()
            .Where(o => o.Id == orderId)
            .Select(o => o.Status)
            .SingleAsync();
    }

    private async Task PrepareAcceptedAndDispatchedAsync(Order order)
    {
        await Mediator.Send(new AcceptOrderCommand(order.Id));
        await Mediator.Send(new RecordOrderAllocationSucceededCommand
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId
        });
        await Mediator.Send(new DispatchOrderCommand(order.Id));
    }

    [Test]
    public async Task Should_ship_when_accepted_and_dispatched()
    {
        var order = await PersistOneRandomOrderInNewScopeAsync();
        await PrepareAcceptedAndDispatchedAsync(order);

        var command = new ShipOrderCommand(order.Id);

        var result = await Mediator.Send(command);

        result.ShouldBeSuccess();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(order.Id);
        result.Value!.Status.Should().Be(OrderStatus.Shipped);

        var status = await GetOrderStatusFromDbAsync(ServiceProvider, order.Id);
        status.Should().Be(OrderStatus.Shipped);
    }

    [Test]
    public async Task Should_fail_when_not_dispatched()
    {
        var order = await PersistOneRandomOrderInNewScopeAsync();
        await Mediator.Send(new AcceptOrderCommand(order.Id));
        await Mediator.Send(new RecordOrderAllocationSucceededCommand
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId
        });

        var command = new ShipOrderCommand(order.Id);

        var result = await Mediator.Send(command);

        result.ShouldBeFailure(typeof(BusinessLogicException));
    }

    [Test]
    public async Task Should_fail_when_order_not_found()
    {
        var command = new ShipOrderCommand(Guid.NewGuid().ToString());

        var result = await Mediator.Send(command);

        result.ShouldBeFailure(typeof(NotFoundException));
    }

    [Test]
    public async Task Should_be_idempotent_when_already_shipped()
    {
        var order = await PersistOneRandomOrderInNewScopeAsync();
        await PrepareAcceptedAndDispatchedAsync(order);

        var first = await Mediator.Send(new ShipOrderCommand(order.Id));
        first.ShouldBeSuccess();

        var second = await Mediator.Send(new ShipOrderCommand(order.Id));

        second.ShouldBeSuccess();
        second.Value!.Status.Should().Be(OrderStatus.Shipped);

        var status = await GetOrderStatusFromDbAsync(ServiceProvider, order.Id);
        status.Should().Be(OrderStatus.Shipped);
    }
}
