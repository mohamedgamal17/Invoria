using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.Ordering.Application.Tests.Assertions;
using Invoria.Ordering.Contracts.Orders.Services;
using Invoria.Ordering.Domain;
using Invoria.Ordering.Domain.Orders;
using Invoria.Ordering.Infrastructure.EntityFramework;
using Invoria.Ordering.Tests.Fakes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Ordering.Application.Tests.Orders.Services;

[TestFixture]
public class OrderServiceTests : OrderTestFixture
{
    private readonly IOrderService _orderService;
    private readonly IOrderingRepository<Order> _orderRepository;

    public OrderServiceTests()
    {
        _orderService = ServiceProvider.GetRequiredService<IOrderService>();
        _orderRepository = ServiceProvider.GetRequiredService<IOrderingRepository<Order>>();
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
    public async Task GetOrderByIdAsync_should_return_public_order_dto_when_found()
    {
        var persistedOrders = await OrderTestData.PersistRandomOrdersAsync(_orderRepository, 1);
        var order = persistedOrders.Single();

        var result = await _orderService.GetOrderByIdAsync(order.Id);

        result.ShouldBeSuccess();
        result.Value.Should().NotBeNull();
        result.Value!.AssertPublicOrderDto(order);
    }

    [Test]
    public async Task GetOrderByIdAsync_should_return_failure_when_order_not_found()
    {
        var nonExistentId = Guid.NewGuid().ToString();

        var result = await _orderService.GetOrderByIdAsync(nonExistentId);

        result.IsSuccess.Should().BeFalse();
        result.Exception.Should().NotBeNull();
        result.Exception.Should().BeOfType<NotFoundException>();
    }

    [Test]
    public async Task ListOrdersByIdsAsync_should_return_all_matching_orders()
    {
        var persistedOrders = await OrderTestData.PersistRandomOrdersAsync(_orderRepository, 2);

        var result = await _orderService.ListOrdersByIdsAsync(persistedOrders.Select(o => o.Id));

        result.ShouldBeSuccess();
        result.Value.Should().HaveCount(2);
        var byNumber = result.Value!.ToDictionary(d => d.OrderNumber);
        foreach (var order in persistedOrders)
        {
            byNumber[order.OrderNumber].AssertPublicOrderDto(order);
        }
    }

    [Test]
    public async Task ListOrdersByIdsAsync_should_omit_missing_ids()
    {
        var persistedOrders = await OrderTestData.PersistRandomOrdersAsync(_orderRepository, 1);
        var missingId = Guid.NewGuid().ToString();

        var result = await _orderService.ListOrdersByIdsAsync(new[] { persistedOrders[0].Id, missingId });

        result.ShouldBeSuccess();
        result.Value.Should().ContainSingle();
        result.Value![0].AssertPublicOrderDto(persistedOrders[0]);
    }

    [Test]
    public async Task ListOrdersByIdsAsync_should_return_empty_when_no_ids()
    {
        var result = await _orderService.ListOrdersByIdsAsync(Array.Empty<string>());

        result.ShouldBeSuccess();
        result.Value.Should().BeEmpty();
    }

    [Test]
    public async Task ListOrdersByIdsAsync_should_return_empty_when_all_ids_whitespace()
    {
        var result = await _orderService.ListOrdersByIdsAsync(new[] { "", "   ", "\t" });

        result.ShouldBeSuccess();
        result.Value.Should().BeEmpty();
    }

    [Test]
    public async Task ListOrdersByIdsAsync_should_deduplicate_ids()
    {
        var persistedOrders = await OrderTestData.PersistRandomOrdersAsync(_orderRepository, 1);

        var result = await _orderService.ListOrdersByIdsAsync(new[] { persistedOrders[0].Id, persistedOrders[0].Id });

        result.ShouldBeSuccess();
        result.Value.Should().ContainSingle();
        result.Value![0].AssertPublicOrderDto(persistedOrders[0]);
    }
}
