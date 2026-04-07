using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.Ordering.Application.Orders.Commands.RefuseOrder;
using Invoria.Ordering.Domain;
using Invoria.Ordering.Domain.Orders;
using Invoria.Ordering.Infrastructure.EntityFramework;
using Invoria.Ordering.Tests.Fakes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Ordering.Application.Tests.Integration.Commands;

[TestFixture]
public class RefuseOrderCommandHandlerTests : OrderTestFixture
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

    [Test]
    public async Task Should_refuse_order_when_accepted()
    {
        var order = await PersistOneRandomOrderInNewScopeAsync();
        await SetOrderStatusAsync(ServiceProvider, order.Id, OrderStatus.Accepted);

        var command = new RefuseOrderCommand(order.Id);

        var result = await Mediator.Send(command);

        result.ShouldBeSuccess();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(order.Id);

        var status = await GetOrderStatusFromDbAsync(ServiceProvider, order.Id);
        status.Should().Be(OrderStatus.Refused);

        var fulfillment = await GetFullfillmentStatusFromDbAsync(ServiceProvider, order.Id);
        fulfillment.Should().Be(FullfillmentStatus.Cancelled);
    }

    [Test]
    public async Task Should_refuse_order_when_completed()
    {
        var order = await PersistOneRandomOrderInNewScopeAsync();
        await SetOrderStatusAsync(ServiceProvider, order.Id, OrderStatus.Completed);

        var command = new RefuseOrderCommand(order.Id);

        var result = await Mediator.Send(command);

        result.ShouldBeSuccess();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(order.Id);

        var status = await GetOrderStatusFromDbAsync(ServiceProvider, order.Id);
        status.Should().Be(OrderStatus.Refused);
    }

    [Test]
    [TestCase(OrderStatus.Pending)]
    [TestCase(OrderStatus.Reopened)]
    [TestCase(OrderStatus.Cancelled)]
    [TestCase(OrderStatus.Refused)]
    public async Task Should_fail_when_order_is_not_accepted_or_completed(OrderStatus status)
    {
        var order = await PersistOneRandomOrderInNewScopeAsync();
        await SetOrderStatusAsync(ServiceProvider, order.Id, status);

        var command = new RefuseOrderCommand(order.Id);

        var result = await Mediator.Send(command);

        result.ShouldBeFailure(typeof(BusinessLogicException));
    }

    [Test]
    public async Task Should_fail_when_order_not_found()
    {
        var command = new RefuseOrderCommand(Guid.NewGuid().ToString());

        var result = await Mediator.Send(command);

        result.ShouldBeFailure(typeof(NotFoundException));
    }
}

