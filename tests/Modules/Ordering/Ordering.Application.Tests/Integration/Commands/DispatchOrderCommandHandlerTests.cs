using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.Ordering.Application.Orders.Commands.AcceptOrder;
using Invoria.Ordering.Application.Orders.Commands.DispatchOrder;
using Invoria.Ordering.Application.Orders.Commands.RecordOrderAllocationSucceeded;
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
public class DispatchOrderCommandHandlerTests : OrderTestFixture
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
    public async Task Should_dispatch_when_accepted_and_allocated_and_publish_integration_event()
    {
        var order = await PersistOneRandomOrderInNewScopeAsync();
        await Mediator.Send(new AcceptOrderCommand(order.Id));
        await Mediator.Send(new RecordOrderAllocationSucceededCommand
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId
        });

        var command = new DispatchOrderCommand(order.Id);

        var result = await Mediator.Send(command);

        result.ShouldBeSuccess();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(order.Id);

        var fulfillment = await GetFullfillmentStatusFromDbAsync(ServiceProvider, order.Id);
        fulfillment.Should().Be(FullfillmentStatus.Dispatched);

        var busMock = ServiceProvider.GetRequiredService<Mock<IBus>>();
        busMock.Verify(
            b => b.Publish(
                It.Is<OrderDispatchedIntegrationEvent>(e =>
                    e.Id == order.Id &&
                    e.OrderNumber == order.OrderNumber &&
                    e.CustomerId == order.CustomerId &&
                    e.Items.Count == order.Items.Count),
                It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }

    [Test]
    public async Task Should_fail_when_not_allocated()
    {
        var order = await PersistOneRandomOrderInNewScopeAsync();
        await Mediator.Send(new AcceptOrderCommand(order.Id));

        var command = new DispatchOrderCommand(order.Id);

        var result = await Mediator.Send(command);

        result.ShouldBeFailure(typeof(BusinessLogicException));

        var busMock = ServiceProvider.GetRequiredService<Mock<IBus>>();
        busMock.Verify(
            b => b.Publish(It.IsAny<object>(), It.IsAny<Dictionary<string, string>>()),
            Times.Never);
    }

    [Test]
    public async Task Should_fail_when_order_not_found()
    {
        var command = new DispatchOrderCommand(Guid.NewGuid().ToString());

        var result = await Mediator.Send(command);

        result.ShouldBeFailure(typeof(NotFoundException));
    }
}
