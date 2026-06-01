using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.Ordering.Application.Orders.Commands.AcceptOrder;
using Invoria.Ordering.Application.Orders.Commands.RecordOrderAllocationFailed;
using Invoria.Ordering.Domain;
using Invoria.Ordering.Domain.Orders;
using Invoria.Ordering.Infrastructure.EntityFramework;
using Invoria.Ordering.Tests.Fakes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Ordering.Application.Tests.Integration.Commands;

[TestFixture]
public class RecordOrderAllocationFailedCommandHandlerTests : OrderTestFixture
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

    [Test]
    public async Task Should_cancel_order_when_allocation_fails()
    {
        var order = await PersistOneRandomOrderInNewScopeAsync();
        await Mediator.Send(new AcceptOrderCommand(order.Id));

        var firstItem = order.Items.First();

        var command = new RecordOrderAllocationFailedCommand
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerId = order.CustomerId,
            Reason = "Insufficient stock",
            ItemErrors =
            [
                new RecordOrderAllocationFailedLine
                {
                    ItemId = firstItem.ProductId,
                    QuantityRequested = firstItem.Quantity + 3,
                    QuantityAvailable = firstItem.Quantity,
                    Shortage = 3
                }
            ]
        };

        var result = await Mediator.Send(command);

        result.ShouldBeSuccess();

        await using var scope = ServiceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
        var persisted = await db.Set<Order>()
            .SingleAsync(o => o.Id == order.Id);

        persisted.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Test]
    public async Task Should_fail_when_order_not_found()
    {
        var command = new RecordOrderAllocationFailedCommand
        {
            OrderId = Guid.NewGuid().ToString(),
            OrderNumber = "MISSING-1",
            CustomerId = Guid.NewGuid().ToString(),
            Reason = "missing",
            ItemErrors = []
        };

        var result = await Mediator.Send(command);

        result.ShouldBeFailure(typeof(NotFoundException));
    }

    [Test]
    public async Task Should_fail_when_customer_does_not_match_event()
    {
        var order = await PersistOneRandomOrderInNewScopeAsync();
        await Mediator.Send(new AcceptOrderCommand(order.Id));

        var command = new RecordOrderAllocationFailedCommand
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerId = Guid.NewGuid().ToString(),
            Reason = "Insufficient stock",
            ItemErrors = []
        };

        var result = await Mediator.Send(command);

        result.ShouldBeFailure(typeof(BusinessLogicException));
    }

    [Test]
    public async Task Should_fail_when_order_not_accepted()
    {
        var order = await PersistOneRandomOrderInNewScopeAsync();

        var command = new RecordOrderAllocationFailedCommand
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerId = order.CustomerId,
            Reason = "Insufficient stock",
            ItemErrors = []
        };

        var result = await Mediator.Send(command);

        result.ShouldBeFailure(typeof(BusinessLogicException));
    }
}
