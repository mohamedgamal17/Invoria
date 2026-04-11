using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.Ordering.Application.Orders.Commands.AcceptOrder;
using Invoria.Ordering.Contracts.Events;
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

    private static async Task<Order> LoadOrderWithItemsAsync(IServiceProvider serviceProvider, string orderId)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
        return await db.Set<Order>()
            .Include(o => o.Items)
            .SingleAsync(o => o.Id == orderId);
    }

    private static bool MatchesAllocateEvent(AllocateOrderIntegrationEvent e, Order expected)
    {
        if (e.Id != expected.Id || e.OrderNumber != expected.OrderNumber || e.CustomerId != expected.CustomerId)
        {
            return false;
        }

        if (e.Items.Count != expected.Items.Count)
        {
            return false;
        }

        var byItemId = expected.Items.ToDictionary(i => i.Id);
        foreach (var item in e.Items)
        {
            if (!byItemId.TryGetValue(item.Id, out var line))
            {
                return false;
            }

            if (line.ProductId != item.ProductId || line.Quantity != item.Quantity)
            {
                return false;
            }
        }

        return true;
    }

    [Test]
    public async Task Should_accept_order_when_pending()
    {
        var order = await PersistOneRandomOrderInNewScopeAsync();
        var expectedSnapshot = await LoadOrderWithItemsAsync(ServiceProvider, order.Id);

        var command = new AcceptOrderCommand(order.Id);

        var result = await Mediator.Send(command);

        result.ShouldBeSuccess();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(order.Id);

        var status = await GetOrderStatusFromDbAsync(ServiceProvider, order.Id);
        status.Should().Be(OrderStatus.Accepted);

        var fulfillment = await GetFullfillmentStatusFromDbAsync(ServiceProvider, order.Id);
        fulfillment.Should().Be(FullfillmentStatus.Allocating);

        var busMock = ServiceProvider.GetRequiredService<Mock<IBus>>();
        busMock.Verify(
            b => b.Publish(
                It.Is<AllocateOrderIntegrationEvent>(e => MatchesAllocateEvent(e, expectedSnapshot)),
                It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }

    [Test]
    public async Task Should_accept_order_when_reopened()
    {
        var order = await PersistOneRandomOrderInNewScopeAsync();
        await SetOrderStatusAsync(ServiceProvider, order.Id, OrderStatus.Reopened);
        var expectedSnapshot = await LoadOrderWithItemsAsync(ServiceProvider, order.Id);

        var command = new AcceptOrderCommand(order.Id);

        var result = await Mediator.Send(command);

        result.ShouldBeSuccess();
        result.Value.Should().NotBeNull();

        var status = await GetOrderStatusFromDbAsync(ServiceProvider, order.Id);
        status.Should().Be(OrderStatus.Accepted);

        var fulfillment = await GetFullfillmentStatusFromDbAsync(ServiceProvider, order.Id);
        fulfillment.Should().Be(FullfillmentStatus.Allocating);

        var busMock = ServiceProvider.GetRequiredService<Mock<IBus>>();
        busMock.Verify(
            b => b.Publish(
                It.Is<AllocateOrderIntegrationEvent>(e => MatchesAllocateEvent(e, expectedSnapshot)),
                It.IsAny<Dictionary<string, string>>()),
            Times.Once);
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
