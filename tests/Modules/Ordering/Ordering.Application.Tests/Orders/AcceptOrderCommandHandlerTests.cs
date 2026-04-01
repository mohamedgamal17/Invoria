using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.Ordering.Application.Orders.Commands.AcceptOrder;
using Invoria.Ordering.Domain;
using Invoria.Ordering.Domain.Orders;
using Invoria.Ordering.Domain.Orders.Events;
using Invoria.Ordering.Infrastructure.EntityFramework;
using Invoria.Ordering.Tests.Fakes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Ordering.Application.Tests.Orders;

[TestFixture]
public class AcceptOrderCommandHandlerTests : OrderTestFixture
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
    }

    private async Task ClearOrdersAsync()
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
        var orders = await db.Set<Order>().ToListAsync();
        db.RemoveRange(orders);
        await db.SaveChangesAsync();
    }

    private static async Task SetOrderStatusAsync(
        IServiceProvider serviceProvider,
        string orderId,
        OrderStatus status)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
        var rows = await db.Set<Order>()
            .Where(o => o.Id == orderId)
            .ExecuteUpdateAsync(s => s.SetProperty(o => o.Status, status));

        rows.Should().Be(1, $"order id {orderId} should exist for status update");
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

    private static async Task SetFullfillmentStatusAsync(
        IServiceProvider serviceProvider,
        string orderId,
        FullfillmentStatus fullfillmentStatus)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
        var rows = await db.Set<Order>()
            .Where(o => o.Id == orderId)
            .ExecuteUpdateAsync(s => s.SetProperty(o => o.FullfillmentStatus, fullfillmentStatus));

        rows.Should().Be(1, $"order id {orderId} should exist for fulfillment update");
    }

    private static async Task<FullfillmentStatus> GetFullfillmentStatusFromDbAsync(
        IServiceProvider serviceProvider,
        string orderId)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
        return await db.Set<Order>()
            .Where(o => o.Id == orderId)
            .Select(o => o.FullfillmentStatus)
            .SingleAsync();
    }

    private static void SetOrderIdForTest(Order order, string id)
    {
        typeof(Entity<string>).GetProperty(nameof(Entity<string>.Id))!.SetValue(order, id);
    }

    [Test]
    public async Task Should_accept_order_when_pending()
    {
        var order = await PersistOneRandomOrderInNewScopeAsync();

        var command = new AcceptOrderCommand(order.Id);

        var result = await Mediator.Send(command);

        result.ShouldBeSuccess();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(order.Id);

        var status = await GetOrderStatusFromDbAsync(ServiceProvider, order.Id);
        status.Should().Be(OrderStatus.Accepted);

        var fulfillment = await GetFullfillmentStatusFromDbAsync(ServiceProvider, order.Id);
        fulfillment.Should().Be(FullfillmentStatus.Allocating);
    }

    [Test]
    public async Task Should_accept_order_when_reopened()
    {
        var order = await PersistOneRandomOrderInNewScopeAsync();
        await SetOrderStatusAsync(ServiceProvider, order.Id, OrderStatus.Reopened);

        var command = new AcceptOrderCommand(order.Id);

        var result = await Mediator.Send(command);

        result.ShouldBeSuccess();
        result.Value.Should().NotBeNull();

        var status = await GetOrderStatusFromDbAsync(ServiceProvider, order.Id);
        status.Should().Be(OrderStatus.Accepted);

        var fulfillment = await GetFullfillmentStatusFromDbAsync(ServiceProvider, order.Id);
        fulfillment.Should().Be(FullfillmentStatus.Allocating);
    }

    [Test]
    public async Task Should_fail_when_fullfillment_is_not_pending()
    {
        var order = await PersistOneRandomOrderInNewScopeAsync();
        await SetFullfillmentStatusAsync(ServiceProvider, order.Id, FullfillmentStatus.Allocating);

        var command = new AcceptOrderCommand(order.Id);

        var result = await Mediator.Send(command);

        result.ShouldBeFailure(typeof(BusinessLogicException));
    }

    [Test]
    public void Accept_raises_OrderAcceptedDomainEvent_and_sets_allocating()
    {
        var order = new Order("TEST-1", Guid.NewGuid().ToString());
        order.UpdateItems(
        [
            new OrderItem(Guid.NewGuid().ToString(), 1, 10m)
        ]);
        SetOrderIdForTest(order, Guid.NewGuid().ToString());

        order.Accept();

        order.FullfillmentStatus.Should().Be(FullfillmentStatus.Allocating);
        order.Status.Should().Be(OrderStatus.Accepted);
        order.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<OrderAcceptedDomainEvent>()
            .Which.OrderId.Should().Be(order.Id);
    }

    [Test]
    [TestCase(OrderStatus.Accepted)]
    [TestCase(OrderStatus.Completed)]
    [TestCase(OrderStatus.Cancelled)]
    public async Task Should_fail_when_order_is_not_pending_or_reopened(OrderStatus status)
    {
        var order = await PersistOneRandomOrderInNewScopeAsync();
        await SetOrderStatusAsync(ServiceProvider, order.Id, status);

        var command = new AcceptOrderCommand(order.Id);

        var result = await Mediator.Send(command);

        result.ShouldBeFailure(typeof(BusinessLogicException));
    }

    [Test]
    public async Task Should_fail_when_order_not_found()
    {
        var command = new AcceptOrderCommand(Guid.NewGuid().ToString());

        var result = await Mediator.Send(command);

        result.ShouldBeFailure(typeof(NotFoundException));
    }
}
