using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.Ordering.Application.Orders.Commands.AcceptOrder;
using Invoria.Ordering.Application.Orders.Commands.AddReturnItems;
using Invoria.Ordering.Application.Orders.Commands.DispatchOrder;
using Invoria.Ordering.Application.Orders.Commands.RecordOrderAllocationSucceeded;
using Invoria.Ordering.Application.Orders.Commands.ShipOrder;
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
public class AddReturnItemsCommandHandlerTests : OrderTestFixture
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

    private async Task PrepareShippedOrderAsync(Order order)
    {
        await Mediator.Send(new AcceptOrderCommand(order.Id));
        await Mediator.Send(new RecordOrderAllocationSucceededCommand
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId
        });
        await Mediator.Send(new DispatchOrderCommand(order.Id));
        await Mediator.Send(new ShipOrderCommand(order.Id));
    }

    private static async Task<string> GetFirstOrderLineIdAsync(
        IServiceProvider serviceProvider,
        string orderId)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
        return await db.Set<Order>()
            .Where(o => o.Id == orderId)
            .SelectMany(o => o.Items)
            .Select(i => i.Id)
            .FirstAsync();
    }

    [Test]
    public async Task Should_record_return_items_when_shipped()
    {
        var order = await PersistOneRandomOrderInNewScopeAsync();
        await PrepareShippedOrderAsync(order);
        var lineId = await GetFirstOrderLineIdAsync(ServiceProvider, order.Id);

        var command = new AddReturnItemsCommand(order.Id, [new AddReturnItemLine(lineId, 1)]);

        var result = await Mediator.Send(command);

        result.ShouldBeSuccess();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(order.Id);
    }

    [Test]
    public async Task Should_fail_when_order_not_shipped()
    {
        var order = await PersistOneRandomOrderInNewScopeAsync();
        var lineId = await GetFirstOrderLineIdAsync(ServiceProvider, order.Id);

        var result = await Mediator.Send(
            new AddReturnItemsCommand(order.Id, [new AddReturnItemLine(lineId, 1)]));

        result.ShouldBeFailure(typeof(BusinessValidationException));
    }

    [Test]
    public async Task Should_fail_when_unknown_order_line()
    {
        var order = await PersistOneRandomOrderInNewScopeAsync();
        await PrepareShippedOrderAsync(order);

        var result = await Mediator.Send(
            new AddReturnItemsCommand(order.Id, [new AddReturnItemLine(Guid.NewGuid().ToString(), 1)]));

        result.ShouldBeFailure(typeof(BusinessValidationException));
    }

    [Test]
    public async Task Should_fail_when_return_quantity_exceeds_ordered()
    {
        var order = await PersistOneRandomOrderInNewScopeAsync();
        await PrepareShippedOrderAsync(order);
        var lineId = await GetFirstOrderLineIdAsync(ServiceProvider, order.Id);

        await using var scope = ServiceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
        var lineQuantity = await db.Set<Order>()
            .Where(o => o.Id == order.Id)
            .SelectMany(o => o.Items)
            .Where(i => i.Id == lineId)
            .Select(i => i.Quantity)
            .SingleAsync();

        var result = await Mediator.Send(
            new AddReturnItemsCommand(order.Id, [new AddReturnItemLine(lineId, lineQuantity + 1)]));

        result.ShouldBeFailure(typeof(BusinessValidationException));
    }

    [Test]
    public async Task Should_fail_when_order_not_found()
    {
        var result = await Mediator.Send(
            new AddReturnItemsCommand(Guid.NewGuid().ToString(), []));

        result.ShouldBeFailure(typeof(NotFoundException));
    }

    [Test]
    public async Task Should_clear_returns_when_items_empty()
    {
        var order = await PersistOneRandomOrderInNewScopeAsync();
        await PrepareShippedOrderAsync(order);
        var lineId = await GetFirstOrderLineIdAsync(ServiceProvider, order.Id);

        var recordResult = await Mediator.Send(
            new AddReturnItemsCommand(order.Id, [new AddReturnItemLine(lineId, 1)]));
        recordResult.ShouldBeSuccess();

        var clearResult = await Mediator.Send(new AddReturnItemsCommand(order.Id, []));
        clearResult.ShouldBeSuccess();
        clearResult.Value!.Id.Should().Be(order.Id);
    }
}
