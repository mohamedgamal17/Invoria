using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.Ordering.Application.Orders.Queries.GetOrderById;
using Invoria.Ordering.Application.Tests.Assertions;
using Invoria.Ordering.Domain;
using Invoria.Ordering.Domain.Orders;
using Invoria.Ordering.Infrastructure.EntityFramework;
using Invoria.Ordering.Tests.Fakes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Ordering.Application.Tests.Integration.Queries;

[TestFixture]
public class GetOrderByIdQueryHandlerTests : OrderTestFixture
{
    private IOrderingRepository<Order> OrderRepository =>
        ServiceProvider.GetRequiredService<IOrderingRepository<Order>>();

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
    public async Task Should_return_order_with_line_items_when_found()
    {
        var order = (await OrderTestData.PersistRandomOrdersAsync(OrderRepository, 1)).Single();

        var query = new GetOrderByIdQuery { Id = order.Id };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        result.Value.Should().NotBeNull();
        result.Value!.AssertOrderDto(order);
    }

    [Test]
    public async Task Should_return_failure_when_order_not_found()
    {
        var nonExistentId = Guid.NewGuid().ToString();
        var query = new GetOrderByIdQuery { Id = nonExistentId };

        var result = await Mediator.Send(query);

        result.IsSuccess.Should().BeFalse();
        result.Exception.Should().NotBeNull();
        result.Exception.Should().BeOfType<NotFoundException>();
    }

    [Test]
    public async Task Should_return_order_with_failure_details_when_present()
    {
        var order = (await OrderTestData.PersistRandomOrdersAsync(OrderRepository, 1)).Single();
        order.Accept();
        var firstItem = order.Items.First();
        order.CancelDueToAllocationFailure("Insufficient stock");
        order.ReplaceFailureDetails(
        [
            new OrderFailureDetails(
                firstItem.ProductId,
                firstItem.Quantity + 2,
                firstItem.Quantity,
                2)
        ]);
        await OrderRepository.Update(order, CancellationToken.None);

        var query = new GetOrderByIdQuery { Id = order.Id };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        var dto = result.Value!;
        dto.FailureDetails.Should().HaveCount(1);
        dto.FailureDetails[0].ItemId.Should().Be(firstItem.ProductId);
        dto.FailureDetails[0].QuantityRequested.Should().Be(firstItem.Quantity + 2);
        dto.FailureDetails[0].QuantityAvailable.Should().Be(firstItem.Quantity);
        dto.FailureDetails[0].Shortage.Should().Be(2);
    }
}
