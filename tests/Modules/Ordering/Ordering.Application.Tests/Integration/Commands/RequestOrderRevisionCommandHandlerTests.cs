using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.Ordering.Application.Orders.Commands.AcceptOrder;
using Invoria.Ordering.Application.Orders.Commands.MarkOrderAllocated;
using Invoria.Ordering.Application.Orders.Commands.RecordOrderAllocation;
using Invoria.Ordering.Application.Orders.Commands.RequestOrderRevision;
using Invoria.Ordering.Contracts.Orders.Enums;
using Invoria.Ordering.Contracts.Orders.Events;
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
public class RequestOrderRevisionCommandHandlerTests : OrderTestFixture
{
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

    [Test]
    public async Task RequestOrderRevision_sets_revision_pending_and_clears_allocated_when_order_is_allocated()
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<IOrderingRepository<Order>>();
        var order = (await OrderTestData.PersistRandomOrdersAsync(repo, 1)).Single();

        (await Mediator.Send(new AcceptOrderCommand(order.Id!))).IsSuccess.Should().BeTrue();
        (await Mediator.Send(new RecordOrderAllocationCommand(order.Id!, "alloc-1"))).IsSuccess.Should().BeTrue();
        (await Mediator.Send(new MarkOrderAllocatedCommand(order.Id!))).IsSuccess.Should().BeTrue();

        var result = await Mediator.Send(new RequestOrderRevisionCommand(order.Id!));

        result.ShouldBeSuccess();
        result.Value!.Status.Should().Be(OrderStatus.RevisionPending);

        await using var verifyScope = ServiceProvider.CreateAsyncScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<OrderingDbContext>();
        var persisted = await db.Set<Order>().SingleAsync(o => o.Id == order.Id);
        persisted.Status.Should().Be(OrderStatus.RevisionPending);
        persisted.OrderAllocated.Should().BeFalse();
        persisted.AllocationId.Should().Be("alloc-1");

        var busMock = ServiceProvider.GetRequiredService<Mock<IBus>>();
        busMock.Verify(
            b => b.Publish(
                It.Is<OrderRevisionRequestedIntegrationEvent>(e =>
                    e.Order.Id == order.Id &&
                    e.AllocationId == "alloc-1" &&
                    e.Order.OrderStatus == OrderStatus.RevisionPending),
                It.IsAny<Dictionary<string, string>>()),
            Times.Once);
    }

    [Test]
    public async Task RequestOrderRevision_fails_when_order_not_found()
    {
        var result = await Mediator.Send(new RequestOrderRevisionCommand(Guid.NewGuid().ToString()));

        result.ShouldBeFailure(typeof(NotFoundException));
    }

    [Test]
    public async Task RequestOrderRevision_fails_when_order_is_not_processing()
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<IOrderingRepository<Order>>();
        var order = (await OrderTestData.PersistRandomOrdersAsync(repo, 1)).Single();

        var result = await Mediator.Send(new RequestOrderRevisionCommand(order.Id!));

        result.ShouldBeFailure(typeof(BusinessLogicException));
    }

    [Test]
    public async Task RequestOrderRevision_fails_when_order_is_not_allocated()
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<IOrderingRepository<Order>>();
        var order = (await OrderTestData.PersistRandomOrdersAsync(repo, 1)).Single();

        (await Mediator.Send(new AcceptOrderCommand(order.Id!))).IsSuccess.Should().BeTrue();

        var result = await Mediator.Send(new RequestOrderRevisionCommand(order.Id!));

        result.ShouldBeFailure(typeof(BusinessLogicException));
    }
}
