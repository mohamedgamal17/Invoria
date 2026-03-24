using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.Ordering.Application.Orders.Queries.ListOrders;
using Invoria.Ordering.Application.Tests.Assertions;
using Invoria.Ordering.Domain;
using Invoria.Ordering.Domain.Orders;
using Invoria.Ordering.Infrastructure.EntityFramework;
using Invoria.Ordering.Tests.Fakes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Ordering.Application.Tests.Orders;

[TestFixture]
public class ListOrdersQueryHandlerTests : OrderTestFixture
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
    public async Task Should_return_empty_page_when_no_orders()
    {
        var query = new ListOrdersQuery { Skip = 0, Length = 10 };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        result.Value.Should().NotBeNull();
        result.Value!.AssertPagingDto(0, 10, 0, 0);
    }

    [Test]
    public async Task Should_return_paged_orders_without_line_items_by_default()
    {
        var persisted = await OrderTestData.PersistRandomOrdersAsync(OrderRepository, 2);

        var query = new ListOrdersQuery { Skip = 0, Length = 10 };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        result.Value.Should().NotBeNull();
        var page = result.Value!;
        page.AssertPagingDto(0, 10, 2, 2);

        foreach (var order in persisted)
        {
            var dto = page.Data.Single(d => d.Id == order.Id);
            dto.CustomerId.Should().Be(order.CustomerId);
            dto.AssertOrderCustomer(expectedCustomer: null);
            dto.OrderNumber.Should().Be(order.OrderNumber);
            dto.Items.Should().BeEmpty();
        }
    }

    [Test]
    public async Task Should_return_line_items_when_include_order_items_is_true()
    {
        var persisted = await OrderTestData.PersistRandomOrdersAsync(OrderRepository, 2);

        var query = new ListOrdersQuery { Skip = 0, Length = 10, IncludeOrderItems = true };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        result.Value.Should().NotBeNull();
        var page = result.Value!;
        page.AssertPagingDto(0, 10, 2, 2);

        foreach (var order in persisted)
        {
            var dto = page.Data.Single(d => d.Id == order.Id);
            dto.CustomerId.Should().Be(order.CustomerId);
            dto.AssertOrderCustomer(expectedCustomer: null);
            dto.OrderNumber.Should().Be(order.OrderNumber);
            dto.Items.Should().HaveCount(order.Items.Count);
            foreach (var item in order.Items)
            {
                var line = dto.Items.Single(i => i.ProductId == item.ProductId);
                line.Quantity.Should().Be(item.Quantity);
                line.Price.Should().Be(item.Price);
            }
        }
    }

    [Test]
    public async Task Should_filter_by_order_number_prefix()
    {
        var customerId = Guid.NewGuid().ToString();
        var orderAlpha = new Order("ALPHA-001", customerId);
        orderAlpha.UpdateItems(new List<OrderItem> { new(Guid.NewGuid().ToString(), 1, 1m) });
        await OrderRepository.Add(orderAlpha, CancellationToken.None);

        var orderBeta = new Order("BETA-002", customerId);
        orderBeta.UpdateItems(new List<OrderItem> { new(Guid.NewGuid().ToString(), 1, 2m) });
        await OrderRepository.Add(orderBeta, CancellationToken.None);

        var query = new ListOrdersQuery
        {
            Skip = 0,
            Length = 10,
            OrderNumber = "ALPHA"
        };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        var page = result.Value!;
        page.AssertPagingDto(0, 10, 1, 1);
        page.Data.Single().Id.Should().Be(orderAlpha.Id);
    }

    [Test]
    public async Task Should_return_no_orders_when_order_number_matches_nothing()
    {
        await OrderTestData.PersistRandomOrdersAsync(OrderRepository, 1);

        var query = new ListOrdersQuery
        {
            Skip = 0,
            Length = 10,
            OrderNumber = "NO-SUCH-TERM-XYZ"
        };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        var page = result.Value!;
        page.AssertPagingDto(0, 10, 0, 0);
        page.Data.Should().BeEmpty();
    }

    [Test]
    public async Task Should_treat_whitespace_only_order_number_as_no_filter()
    {
        var persisted = await OrderTestData.PersistRandomOrdersAsync(OrderRepository, 2);

        var query = new ListOrdersQuery
        {
            Skip = 0,
            Length = 10,
            OrderNumber = "   \t  "
        };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        var page = result.Value!;
        page.AssertPagingDto(0, 10, 2, 2);
        foreach (var order in persisted)
        {
            var dto = page.Data.Single(d => d.Id == order.Id);
            dto.OrderNumber.Should().Be(order.OrderNumber);
            dto.AssertOrderCustomer(expectedCustomer: null);
            dto.Items.Should().BeEmpty();
        }
    }

    [Test]
    public async Task Should_trim_order_number_when_searching()
    {
        var customerId = Guid.NewGuid().ToString();
        var orderAlpha = new Order("ALPHA-001", customerId);
        orderAlpha.UpdateItems(new List<OrderItem> { new(Guid.NewGuid().ToString(), 1, 1m) });
        await OrderRepository.Add(orderAlpha, CancellationToken.None);

        var orderBeta = new Order("BETA-002", customerId);
        orderBeta.UpdateItems(new List<OrderItem> { new(Guid.NewGuid().ToString(), 1, 2m) });
        await OrderRepository.Add(orderBeta, CancellationToken.None);

        var query = new ListOrdersQuery
        {
            Skip = 0,
            Length = 10,
            OrderNumber = "  ALPHA  "
        };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        var page = result.Value!;
        page.AssertPagingDto(0, 10, 1, 1);
        page.Data.Single().Id.Should().Be(orderAlpha.Id);
    }

    [Test]
    public async Task Should_return_line_items_when_filtering_by_order_number_with_include_order_items()
    {
        var customerId = Guid.NewGuid().ToString();
        var orderAlpha = new Order("ALPHA-X", customerId);
        var pidAlpha = Guid.NewGuid().ToString();
        orderAlpha.UpdateItems(new List<OrderItem> { new(pidAlpha, 3, 12.5m) });
        await OrderRepository.Add(orderAlpha, CancellationToken.None);

        var orderBeta = new Order("BETA-Y", customerId);
        orderBeta.UpdateItems(new List<OrderItem> { new(Guid.NewGuid().ToString(), 1, 99m) });
        await OrderRepository.Add(orderBeta, CancellationToken.None);

        var query = new ListOrdersQuery
        {
            Skip = 0,
            Length = 10,
            OrderNumber = "ALPHA",
            IncludeOrderItems = true
        };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        var page = result.Value!;
        page.AssertPagingDto(0, 10, 1, 1);
        var dto = page.Data.Single();
        dto.Id.Should().Be(orderAlpha.Id);
        dto.AssertOrderCustomer(expectedCustomer: null);
        dto.Items.Should().HaveCount(1);
        dto.Items[0].ProductId.Should().Be(pidAlpha);
        dto.Items[0].Quantity.Should().Be(3);
        dto.Items[0].Price.Should().Be(12.5m);
    }

    [Test]
    public async Task Should_filter_by_numeric_prefix_of_order_number()
    {
        var customerId = Guid.NewGuid().ToString();
        var orderMatch = new Order("120240001", customerId);
        orderMatch.UpdateItems(new List<OrderItem> { new(Guid.NewGuid().ToString(), 1, 1m) });
        await OrderRepository.Add(orderMatch, CancellationToken.None);

        var orderOther = new Order("990010001", customerId);
        orderOther.UpdateItems(new List<OrderItem> { new(Guid.NewGuid().ToString(), 1, 1m) });
        await OrderRepository.Add(orderOther, CancellationToken.None);

        var query = new ListOrdersQuery
        {
            Skip = 0,
            Length = 10,
            OrderNumber = "12"
        };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        var page = result.Value!;
        page.AssertPagingDto(0, 10, 1, 1);
        page.Data.Single().Id.Should().Be(orderMatch.Id);
    }
}
