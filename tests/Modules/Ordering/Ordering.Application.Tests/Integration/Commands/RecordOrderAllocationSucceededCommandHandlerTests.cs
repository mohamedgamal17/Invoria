using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.Ordering.Application.Orders.Commands.AcceptOrder;
using Invoria.Ordering.Application.Orders.Commands.RecordOrderAllocationSucceeded;
using Invoria.Ordering.Domain;
using Invoria.Ordering.Domain.Orders;
using Invoria.Ordering.Infrastructure.EntityFramework;
using Invoria.Ordering.Tests.Fakes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Ordering.Application.Tests.Integration.Commands;

[TestFixture]
public class RecordOrderAllocationSucceededCommandHandlerTests : OrderTestFixture
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
    public async Task Should_succeed_when_accepted()
    {
        var order = await PersistOneRandomOrderInNewScopeAsync();
        await Mediator.Send(new AcceptOrderCommand(order.Id));

        var result = await Mediator.Send(new RecordOrderAllocationSucceededCommand
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId
        });

        result.ShouldBeSuccess();
    }

    [Test]
    public async Task Should_succeed_idempotently_when_called_twice()
    {
        var order = await PersistOneRandomOrderInNewScopeAsync();
        await Mediator.Send(new AcceptOrderCommand(order.Id));

        var command = new RecordOrderAllocationSucceededCommand
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId
        };

        (await Mediator.Send(command)).ShouldBeSuccess();
        (await Mediator.Send(command)).ShouldBeSuccess();
    }

    [Test]
    public async Task Should_fail_when_order_not_found()
    {
        var result = await Mediator.Send(new RecordOrderAllocationSucceededCommand
        {
            OrderId = Guid.NewGuid().ToString(),
            CustomerId = Guid.NewGuid().ToString()
        });

        result.ShouldBeFailure(typeof(NotFoundException));
    }

    [Test]
    public async Task Should_fail_when_customer_does_not_match_event()
    {
        var order = await PersistOneRandomOrderInNewScopeAsync();
        await Mediator.Send(new AcceptOrderCommand(order.Id));

        var result = await Mediator.Send(new RecordOrderAllocationSucceededCommand
        {
            OrderId = order.Id,
            CustomerId = Guid.NewGuid().ToString()
        });

        result.ShouldBeFailure(typeof(BusinessLogicException));
    }

    [Test]
    public async Task Should_fail_when_order_not_accepted()
    {
        var order = await PersistOneRandomOrderInNewScopeAsync();

        var result = await Mediator.Send(new RecordOrderAllocationSucceededCommand
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId
        });

        result.ShouldBeFailure(typeof(BusinessLogicException));
    }
}
