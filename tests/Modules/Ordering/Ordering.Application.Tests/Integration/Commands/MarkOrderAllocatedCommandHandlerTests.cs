using FluentAssertions;
using Invoria.Ordering.Application.Orders.Commands.AcceptOrder;
using Invoria.Ordering.Application.Orders.Commands.MarkOrderAllocated;
using Invoria.Ordering.Application.Orders.Commands.RecordOrderAllocation;
using Invoria.Ordering.Contracts.Orders.Enums;
using Invoria.Ordering.Domain;
using Invoria.Ordering.Domain.Orders;
using Invoria.Ordering.Infrastructure.EntityFramework;
using Invoria.Ordering.Tests.Fakes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Ordering.Application.Tests.Integration.Commands;

[TestFixture]
public class MarkOrderAllocatedCommandHandlerTests : OrderTestFixture
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
    public async Task MarkOrderAllocated_sets_order_allocated_true_when_order_is_processing()
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<IOrderingRepository<Order>>();
        var order = (await OrderTestData.PersistRandomOrdersAsync(repo, 1)).Single();

        (await Mediator.Send(new AcceptOrderCommand(order.Id!))).IsSuccess.Should().BeTrue();
        (await Mediator.Send(new RecordOrderAllocationCommand(order.Id!, "alloc-1"))).IsSuccess.Should().BeTrue();

        var result = await Mediator.Send(new MarkOrderAllocatedCommand(order.Id!));

        result.IsSuccess.Should().BeTrue();

        await using var verifyScope = ServiceProvider.CreateAsyncScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<OrderingDbContext>();
        var persisted = await db.Set<Order>().SingleAsync(o => o.Id == order.Id);
        persisted.OrderAllocated.Should().BeTrue();
        persisted.Status.Should().Be(OrderStatus.Processing);
        persisted.AllocationId.Should().Be("alloc-1");
    }
}
