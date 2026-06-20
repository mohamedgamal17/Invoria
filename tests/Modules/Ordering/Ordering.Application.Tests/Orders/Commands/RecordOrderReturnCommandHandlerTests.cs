using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.Ordering.Application.Orders.Commands.AcceptOrder;
using Invoria.Ordering.Application.Orders.Commands.CompleteOrder;
using Invoria.Ordering.Application.Orders.Commands.RecordOrderReturn;
using Invoria.Ordering.Contracts.Orders.Enums;
using Invoria.Ordering.Domain;
using Invoria.Ordering.Domain.Orders;
using Invoria.Ordering.Infrastructure.EntityFramework;
using Invoria.Ordering.Tests.Fakes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Ordering.Application.Tests.Orders.Commands;

[TestFixture]
public class RecordOrderReturnCommandHandlerTests : OrderTestFixture
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
    public async Task RecordOrderReturn_persists_return_id_when_order_is_completed()
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<IOrderingRepository<Order>>();
        var order = (await OrderTestData.PersistRandomOrdersAsync(repo, 1)).Single();

        (await Mediator.Send(new AcceptOrderCommand(order.Id!))).IsSuccess.Should().BeTrue();
        (await Mediator.Send(new CompleteOrderCommand(order.Id!))).IsSuccess.Should().BeTrue();

        var result = await Mediator.Send(new RecordOrderReturnCommand(order.Id!, "return-1"));

        result.IsSuccess.Should().BeTrue();

        await using var verifyScope = ServiceProvider.CreateAsyncScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<OrderingDbContext>();
        var persisted = await db.Set<Order>().SingleAsync(o => o.Id == order.Id);
        persisted.ReturnId.Should().Be("return-1");
        persisted.Status.Should().Be(OrderStatus.Completed);
    }
}
