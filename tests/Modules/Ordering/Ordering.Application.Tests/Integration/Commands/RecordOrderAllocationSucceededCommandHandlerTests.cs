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
    public async Task Should_set_fulfillment_allocated_when_accepted_and_allocating()
    {
        var order = await PersistOneRandomOrderInNewScopeAsync();
        await Mediator.Send(new AcceptOrderCommand(order.Id));

        var command = new RecordOrderAllocationSucceededCommand
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId
        };

        var result = await Mediator.Send(command);

        result.ShouldBeSuccess();
        var fulfillment = await GetFullfillmentStatusFromDbAsync(ServiceProvider, order.Id);
        fulfillment.Should().Be(FullfillmentStatus.Allocated);
    }

    [Test]
    public async Task Should_succeed_idempotently_when_already_allocated()
    {
        var order = await PersistOneRandomOrderInNewScopeAsync();
        await Mediator.Send(new AcceptOrderCommand(order.Id));

        var command = new RecordOrderAllocationSucceededCommand
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId
        };

        var first = await Mediator.Send(command);
        first.ShouldBeSuccess();

        var second = await Mediator.Send(command);
        second.ShouldBeSuccess();

        var fulfillment = await GetFullfillmentStatusFromDbAsync(ServiceProvider, order.Id);
        fulfillment.Should().Be(FullfillmentStatus.Allocated);
    }

    [Test]
    public async Task Should_fail_when_order_not_found()
    {
        var missingId = Guid.NewGuid().ToString();
        var command = new RecordOrderAllocationSucceededCommand
        {
            OrderId = missingId,
            CustomerId = Guid.NewGuid().ToString()
        };

        var result = await Mediator.Send(command);

        result.ShouldBeFailure(typeof(NotFoundException));
    }

    [Test]
    public async Task Should_fail_when_customer_does_not_match_event()
    {
        var order = await PersistOneRandomOrderInNewScopeAsync();
        await Mediator.Send(new AcceptOrderCommand(order.Id));

        var command = new RecordOrderAllocationSucceededCommand
        {
            OrderId = order.Id,
            CustomerId = Guid.NewGuid().ToString()
        };

        var result = await Mediator.Send(command);

        result.ShouldBeFailure(typeof(BusinessLogicException));
    }

    [Test]
    public async Task Should_fail_when_order_not_accepted_allocating()
    {
        var order = await PersistOneRandomOrderInNewScopeAsync();

        var command = new RecordOrderAllocationSucceededCommand
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId
        };

        var result = await Mediator.Send(command);

        result.ShouldBeFailure(typeof(BusinessLogicException));
    }
}
