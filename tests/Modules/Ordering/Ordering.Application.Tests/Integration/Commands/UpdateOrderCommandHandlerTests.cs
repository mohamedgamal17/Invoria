using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.Ordering.Application.Orders.Commands.CreateOrder;
using Invoria.Ordering.Application.Orders.Commands.UpdateOrder;
using Invoria.Ordering.Application.Tests.Assertions;
using Invoria.Ordering.Domain;
using Invoria.Ordering.Domain.Orders;
using Invoria.Ordering.Infrastructure.EntityFramework;
using Invoria.Ordering.Tests.Fakes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Ordering.Application.Tests.Integration.Commands;

[TestFixture]
public class UpdateOrderCommandHandlerTests : OrderTestFixture
{
    /// <summary>
    /// Persists inside a dedicated scope so the DbContext is disposed before Mediator runs,
    /// avoiding a tracked <see cref="Order"/> with stale state when the handler loads the entity.
    /// </summary>
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

    [Test]
    public async Task Should_update_items_when_order_is_pending()
    {
        var order = await PersistOneRandomOrderInNewScopeAsync();

        var newItems = new List<CreateOrderItemCommand>
        {
            new(Guid.NewGuid().ToString(), 3, 15.5m),
            new(Guid.NewGuid().ToString(), 1, 99m)
        };

        var command = new UpdateOrderCommand(order.Id, newItems);

        var result = await Mediator.Send(command);

        result.ShouldBeSuccess();
        result.Value.Should().NotBeNull();
        result.Value!.AssertOrderDto(command);
    }

    [Test]
    public async Task Should_update_items_when_order_is_revision()
    {
        var order = await PersistOneRandomOrderInNewScopeAsync();
        await SetOrderStatusAsync(ServiceProvider, order.Id, OrderStatus.Revision);

        var newItems = new List<CreateOrderItemCommand>
        {
            new(Guid.NewGuid().ToString(), 2, 20m)
        };

        var command = new UpdateOrderCommand(order.Id, newItems);

        var result = await Mediator.Send(command);

        result.ShouldBeSuccess();
        result.Value.Should().NotBeNull();
        result.Value!.AssertOrderDto(command);
    }

    [Test]
    public async Task Should_fail_when_order_is_not_pending_or_revision()
    {
        var order = await PersistOneRandomOrderInNewScopeAsync();
        await SetOrderStatusAsync(ServiceProvider, order.Id, OrderStatus.Processing);

        var command = new UpdateOrderCommand(
            order.Id,
            new List<CreateOrderItemCommand> { new(Guid.NewGuid().ToString(), 1, 10m) });

        var result = await Mediator.Send(command);

        result.ShouldBeFailure(typeof(BusinessLogicException));
    }

    [Test]
    public async Task Should_fail_when_order_not_found()
    {
        var command = new UpdateOrderCommand(
            Guid.NewGuid().ToString(),
            new List<CreateOrderItemCommand> { new(Guid.NewGuid().ToString(), 1, 10m) });

        var result = await Mediator.Send(command);

        result.ShouldBeFailure(typeof(NotFoundException));
    }
}
