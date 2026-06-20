using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.Ordering.Application.Orders.Commands.AcceptOrder;
using Invoria.Ordering.Application.Orders.Commands.RecordOrderAllocation;
using Invoria.Ordering.Contracts.Orders.Enums;
using Invoria.Ordering.Domain;
using Invoria.Ordering.Domain.Orders;
using Invoria.Ordering.Infrastructure.EntityFramework;
using Invoria.Ordering.Tests.Fakes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Ordering.Application.Tests.Orders.Commands;

[TestFixture]
public class RecordOrderAllocationCommandHandlerTests : OrderTestFixture
{
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

    [Test]
    public async Task RecordOrderAllocation_persists_allocation_id_when_order_is_processing()
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<IOrderingRepository<Order>>();
        var order = (await OrderTestData.PersistRandomOrdersAsync(repo, 1)).Single();

        (await Mediator.Send(new AcceptOrderCommand(order.Id!))).IsSuccess.Should().BeTrue();

        var result = await Mediator.Send(new RecordOrderAllocationCommand(order.Id!, "alloc-1"));

        result.IsSuccess.Should().BeTrue();

        await using var verifyScope = ServiceProvider.CreateAsyncScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<OrderingDbContext>();
        var persisted = await db.Set<Order>().SingleAsync(o => o.Id == order.Id);
        persisted.AllocationId.Should().Be("alloc-1");
        persisted.Status.Should().Be(OrderStatus.Processing);
    }

    [Test]
    public async Task RecordOrderAllocation_fails_when_order_not_found()
    {
        var result = await Mediator.Send(new RecordOrderAllocationCommand("missing-order", "alloc-1"));

        result.ShouldBeFailure(typeof(NotFoundException));
    }

    [Test]
    public async Task RecordOrderAllocation_fails_when_order_is_not_processing()
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<IOrderingRepository<Order>>();
        var order = (await OrderTestData.PersistRandomOrdersAsync(repo, 1)).Single();

        var result = await Mediator.Send(new RecordOrderAllocationCommand(order.Id!, "alloc-1"));

        result.ShouldBeFailure(typeof(BusinessLogicException));
    }
}
