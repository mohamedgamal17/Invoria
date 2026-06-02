using Autofac;
using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.Ordering.Application.Orders.Commands.AcceptOrder;
using Invoria.Ordering.Application.Orders.Commands.AddReturnItems;
using Invoria.Ordering.Application.Orders.Queries.GetOrderById;
using Invoria.Ordering.Application.Tests.Assertions;
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
    private async Task<Order> PersistOneRandomOrderAsync()
    {
        return (await OrderTestData.PersistRandomOrdersAsync(OrderRepository, 1)).Single();
    }

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

    private async Task PrepareProcessingOrderAsync(Order order)
    {
        await Mediator.Send(new AcceptOrderCommand(order.Id));
    }

    private async Task<string> GetFirstOrderLineIdAsync(string orderId)
    {
        var db = Scope.Resolve<OrderingDbContext>();
        return await db.Set<Order>()
            .Where(o => o.Id == orderId)
            .SelectMany(o => o.Items)
            .Select(i => i.Id)
            .FirstAsync();
    }

    [Test]
    public async Task Should_record_return_items_when_processing()
    {
        var order = await PersistOneRandomOrderAsync();
        await PrepareProcessingOrderAsync(order);
        var lineId = await GetFirstOrderLineIdAsync(order.Id);

        var command = new AddReturnItemsCommand(order.Id, [new AddReturnItemLine(lineId, 1)]);

        var result = await Mediator.Send(command);

        result.ShouldBeSuccess();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(order.Id);
        result.Value.ReturnItems.Should().ContainSingle();
        result.Value.ReturnItems[0].OrderItemId.Should().Be(lineId);
        result.Value.ReturnItems[0].Quantity.Should().Be(1);
        result.Value.NetOfTotalOrderAmount.Should().BeLessThan(result.Value.TotalOrderAmount);
    }

    [Test]
    public async Task Should_persist_returns_and_reload_via_GetOrderById()
    {
        var order = await PersistOneRandomOrderAsync();
        await PrepareProcessingOrderAsync(order);
        var lineId = await GetFirstOrderLineIdAsync(order.Id);

        var recordResult = await Mediator.Send(
            new AddReturnItemsCommand(order.Id, [new AddReturnItemLine(lineId, 1)]));
        recordResult.ShouldBeSuccess();

        var getResult = await Mediator.Send(new GetOrderByIdQuery { Id = order.Id });

        getResult.ShouldBeSuccess();
        getResult.Value!.ReturnItems.Should().ContainSingle();
        getResult.Value.ReturnItems[0].OrderItemId.Should().Be(lineId);
        getResult.Value.ReturnItems[0].Quantity.Should().Be(1);
        getResult.Value.ReturnsTotal.Should().BeGreaterThan(0);
        getResult.Value.NetOfTotalOrderAmount.Should().Be(getResult.Value.TotalOrderAmount - getResult.Value.ReturnsTotal);
    }

    [Test]
    public async Task Should_record_return_items_when_order_pending()
    {
        var order = await PersistOneRandomOrderAsync();
        var lineId = await GetFirstOrderLineIdAsync(order.Id);

        var result = await Mediator.Send(
            new AddReturnItemsCommand(order.Id, [new AddReturnItemLine(lineId, 1)]));

        result.ShouldBeSuccess();
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
        var order = await PersistOneRandomOrderAsync();
        await PrepareProcessingOrderAsync(order);
        var lineId = await GetFirstOrderLineIdAsync(order.Id);

        var recordResult = await Mediator.Send(
            new AddReturnItemsCommand(order.Id, [new AddReturnItemLine(lineId, 1)]));
        recordResult.ShouldBeSuccess();

        var clearResult = await Mediator.Send(new AddReturnItemsCommand(order.Id, []));
        clearResult.ShouldBeSuccess();
        clearResult.Value!.Id.Should().Be(order.Id);
    }
}
