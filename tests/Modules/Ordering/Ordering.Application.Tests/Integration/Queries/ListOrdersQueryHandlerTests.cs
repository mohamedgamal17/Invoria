using Autofac;
using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Ordering.Application.Orders.Commands.AcceptOrder;
using Invoria.Ordering.Application.Orders.Commands.AddReturnItems;
using Invoria.Ordering.Application.Orders.Queries.ListOrders;
using Invoria.Ordering.Application.Tests.Assertions;
using Invoria.Ordering.Contracts.Orders.Enums;
using Invoria.Ordering.Domain;
using Invoria.Ordering.Domain.Orders;
using Invoria.Ordering.Infrastructure.EntityFramework;
using Invoria.Ordering.Tests.Fakes;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Ordering.Application.Tests.Integration.Queries;

[TestFixture]
public class ListOrdersQueryHandlerTests : OrderTestFixture
{
    private IOrderingRepository<Order> OrderRepository =>
        Scope.Resolve<IOrderingRepository<Order>>();

    private IMediator TestMediator => Scope.Resolve<IMediator>();

    protected override async Task BeforeAnyTestRunAsync()
    {
        await ClearOrdersAsync();
    }

    private async Task ClearOrdersAsync()
    {
        var db = Scope.Resolve<OrderingDbContext>();
        var orders = await db.Set<Order>().ToListAsync();
        db.RemoveRange(orders);
        await db.SaveChangesAsync();
    }

    private static void CompleteOrder(Order order)
    {
        order.Revise();
        order.Complete();
    }

    private async Task<string> PrepareShippedOrderWithReturnAsync(Order order)
    {
        await TestMediator.Send(new AcceptOrderCommand(order.Id));

        var lineId = await GetFirstOrderLineIdAsync(Scope.Resolve<OrderingDbContext>(), order.Id);
        var recordResult = await TestMediator.Send(
            new AddReturnItemsCommand(order.Id, [new AddReturnItemLine(lineId, 1)]));
        recordResult.ShouldBeSuccess();

        return lineId;
    }

    private static async Task<string> GetFirstOrderLineIdAsync(
        OrderingDbContext db,
        string orderId)
    {
        return await db.Set<Order>()
            .Where(o => o.Id == orderId)
            .SelectMany(o => o.Items)
            .Select(i => i.Id)
            .FirstAsync();
    }

    [Test]
    public async Task Should_return_empty_page_when_no_orders()
    {
        var query = new ListOrdersQuery { Skip = 0, Length = 10 };

        var result = await TestMediator.Send(query);

        result.ShouldBeSuccess();
        result.Value.Should().NotBeNull();
        result.Value!.AssertPagingDto(0, 10, 0, 0);
    }

    [Test]
    public async Task Should_return_paged_orders_without_line_items_by_default()
    {
        var persisted = await OrderTestData.PersistRandomOrdersAsync(OrderRepository, 2);

        var query = new ListOrdersQuery { Skip = 0, Length = 10 };

        var result = await TestMediator.Send(query);

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
            dto.ReturnItems.Should().BeEmpty();
            dto.TotalOrderAmount.Should().Be(0);
            dto.NetOfTotalOrderAmount.Should().Be(0);
            dto.ReturnsTotal.Should().Be(0);
        }
    }

    [Test]
    public async Task Should_return_line_items_without_returns_when_include_order_items_only()
    {
        var persisted = await OrderTestData.PersistRandomOrdersAsync(OrderRepository, 1);
        var order = persisted.Single();
        await PrepareShippedOrderWithReturnAsync(order);

        var query = new ListOrdersQuery
        {
            Skip = 0,
            Length = 10,
            IncludeOrderItems = true,
            IncludeReturnItems = false
        };

        var result = await TestMediator.Send(query);

        result.ShouldBeSuccess();
        var dto = result.Value!.Data.Single(d => d.Id == order.Id);
        dto.Items.Should().HaveCount(order.Items.Count);
        dto.ReturnItems.Should().BeEmpty();
        dto.TotalOrderAmount.Should().Be(0);
        dto.NetOfTotalOrderAmount.Should().Be(0);
    }

    [Test]
    public async Task Should_return_returns_and_pricing_without_line_items_when_include_return_items_only()
    {
        var persisted = await OrderTestData.PersistRandomOrdersAsync(OrderRepository, 1);
        var order = persisted.Single();
        var lineId = await PrepareShippedOrderWithReturnAsync(order);

        var query = new ListOrdersQuery
        {
            Skip = 0,
            Length = 10,
            IncludeReturnItems = true
        };

        var result = await TestMediator.Send(query);

        result.ShouldBeSuccess();
        var dto = result.Value!.Data.Single(d => d.Id == order.Id);
        dto.Items.Should().BeEmpty();
        dto.ReturnItems.Should().ContainSingle();
        dto.ReturnItems[0].OrderItemId.Should().Be(lineId);
        dto.NetOfTotalOrderAmount.Should().BeLessThan(dto.TotalOrderAmount);
        dto.ReturnsTotal.Should().BeGreaterThan(0);
    }

    [Test]
    public async Task Should_return_orders_ordered_by_id_descending()
    {
        var persisted = await OrderTestData.PersistRandomOrdersAsync(OrderRepository, 3);

        var query = new ListOrdersQuery { Skip = 0, Length = 3 };

        var result = await TestMediator.Send(query);

        result.ShouldBeSuccess();
        var expectedIds = persisted
            .Select(x => x.Id)
            .OrderByDescending(x => x)
            .ToList();
        result.Value!.Data.Select(x => x.Id).Should().Equal(expectedIds);
    }

    [Test]
    public async Task Should_return_line_items_when_include_order_items_is_true()
    {
        var persisted = await OrderTestData.PersistRandomOrdersAsync(OrderRepository, 2);

        var query = new ListOrdersQuery { Skip = 0, Length = 10, IncludeOrderItems = true };

        var result = await TestMediator.Send(query);

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

        var result = await TestMediator.Send(query);

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

        var result = await TestMediator.Send(query);

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

        var result = await TestMediator.Send(query);

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

        var result = await TestMediator.Send(query);

        result.ShouldBeSuccess();
        var page = result.Value!;
        page.AssertPagingDto(0, 10, 1, 1);
        page.Data.Single().Id.Should().Be(orderAlpha.Id);
    }

    [Test]
    public async Task Should_filter_by_customer_id()
    {
        var targetCustomerId = Guid.NewGuid().ToString();
        var otherCustomerId = Guid.NewGuid().ToString();

        var targetOrder = new Order("CUST-A-001", targetCustomerId);
        targetOrder.UpdateItems(new List<OrderItem> { new(Guid.NewGuid().ToString(), 1, 10m) });
        await OrderRepository.Add(targetOrder, CancellationToken.None);

        var otherOrder = new Order("CUST-B-001", otherCustomerId);
        otherOrder.UpdateItems(new List<OrderItem> { new(Guid.NewGuid().ToString(), 1, 20m) });
        await OrderRepository.Add(otherOrder, CancellationToken.None);

        var query = new ListOrdersQuery
        {
            Skip = 0,
            Length = 10,
            CustomerId = targetCustomerId
        };

        var result = await TestMediator.Send(query);

        result.ShouldBeSuccess();
        var page = result.Value!;
        page.AssertPagingDto(0, 10, 1, 1);
        page.Data.Single().Id.Should().Be(targetOrder.Id);
        page.Data.Should().OnlyContain(x => x.CustomerId == targetCustomerId);
    }

    [Test]
    public async Task Should_treat_whitespace_only_customer_id_as_no_filter()
    {
        var persisted = await OrderTestData.PersistRandomOrdersAsync(OrderRepository, 2);

        var query = new ListOrdersQuery
        {
            Skip = 0,
            Length = 10,
            CustomerId = "   \t  "
        };

        var result = await TestMediator.Send(query);

        result.ShouldBeSuccess();
        var page = result.Value!;
        page.AssertPagingDto(0, 10, 2, 2);
        foreach (var order in persisted)
        {
            page.Data.Should().Contain(x => x.Id == order.Id);
        }
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

        var result = await TestMediator.Send(query);

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

        var result = await TestMediator.Send(query);

        result.ShouldBeSuccess();
        var page = result.Value!;
        page.AssertPagingDto(0, 10, 1, 1);
        page.Data.Single().Id.Should().Be(orderMatch.Id);
    }

    [Test]
    public async Task Should_filter_by_order_status()
    {
        var customerId = Guid.NewGuid().ToString();

        var pendingOrder = new Order("STATUS-PENDING", customerId);
        pendingOrder.UpdateItems([new OrderItem(Guid.NewGuid().ToString(), 1, 10m)]);
        await OrderRepository.Add(pendingOrder, CancellationToken.None);

        var acceptedOrder = new Order("STATUS-ACCEPTED", customerId);
        acceptedOrder.UpdateItems([new OrderItem(Guid.NewGuid().ToString(), 1, 20m)]);
        acceptedOrder.Revise();
        await OrderRepository.Add(acceptedOrder, CancellationToken.None);

        var query = new ListOrdersQuery
        {
            Skip = 0,
            Length = 10,
            Status = OrderStatus.Pending
        };

        var result = await TestMediator.Send(query);

        result.ShouldBeSuccess();
        var page = result.Value!;
        page.Data.Should().ContainSingle(x => x.Id == pendingOrder.Id);
        page.Data.Should().OnlyContain(x => x.Status == OrderStatus.Pending);
    }

    [Test]
    public async Task Should_filter_by_payment_type()
    {
        var customerId = Guid.NewGuid().ToString();

        var immediateOrder = new Order("PAYMENT-TYPE-IMMEDIATE", customerId, OrderPaymentType.Immediate);
        immediateOrder.UpdateItems([new OrderItem(Guid.NewGuid().ToString(), 1, 10m)]);
        await OrderRepository.Add(immediateOrder, CancellationToken.None);

        var debtOrder = new Order("PAYMENT-TYPE-DEBT", customerId, OrderPaymentType.Debt);
        debtOrder.UpdateItems([new OrderItem(Guid.NewGuid().ToString(), 1, 20m)]);
        await OrderRepository.Add(debtOrder, CancellationToken.None);

        var query = new ListOrdersQuery
        {
            Skip = 0,
            Length = 10,
            PaymentType = OrderPaymentType.Debt
        };

        var result = await TestMediator.Send(query);

        result.ShouldBeSuccess();
        var page = result.Value!;
        page.Data.Should().ContainSingle(x => x.Id == debtOrder.Id);
        page.Data.Should().OnlyContain(x => x.PaymentType == OrderPaymentType.Debt);
    }

    [Test]
    public async Task Should_filter_by_payment_status()
    {
        var customerId = Guid.NewGuid().ToString();

        var unpaidOrder = new Order("PAYMENT-STATUS-UNPAID", customerId, OrderPaymentType.Debt);
        unpaidOrder.UpdateItems([new OrderItem(Guid.NewGuid().ToString(), 2, 10m)]);
        await OrderRepository.Add(unpaidOrder, CancellationToken.None);

        var partialOrder = new Order("PAYMENT-STATUS-PARTIAL", customerId, OrderPaymentType.Debt);
        partialOrder.UpdateItems([new OrderItem(Guid.NewGuid().ToString(), 2, 15m)]);
        await OrderRepository.Add(partialOrder, CancellationToken.None);
        CompleteOrder(partialOrder);
        partialOrder.RecordPayment(10m, OrderPaymentMethod.Cash, DateTimeOffset.UtcNow);
        await OrderRepository.Update(partialOrder, CancellationToken.None);

        var paidOrder = new Order("PAYMENT-STATUS-PAID", customerId, OrderPaymentType.Immediate);
        paidOrder.UpdateItems([new OrderItem(Guid.NewGuid().ToString(), 3, 10m)]);
        await OrderRepository.Add(paidOrder, CancellationToken.None);
        CompleteOrder(paidOrder);
        paidOrder.RecordPayment(30m, OrderPaymentMethod.Cash, DateTimeOffset.UtcNow);
        await OrderRepository.Update(paidOrder, CancellationToken.None);

        var query = new ListOrdersQuery
        {
            Skip = 0,
            Length = 10,
            PaymentStatus = OrderPaymentStatus.Partial
        };

        var result = await TestMediator.Send(query);

        result.ShouldBeSuccess();
        var page = result.Value!;
        page.Data.Should().ContainSingle(x => x.Id == partialOrder.Id);
        page.Data.Should().OnlyContain(x => x.PaymentStatus == OrderPaymentStatus.Partial);
    }

    [Test]
    public async Task Should_filter_by_payment_type_and_payment_status_together()
    {
        var customerId = Guid.NewGuid().ToString();

        var targetOrder = new Order("PAYMENT-COMBO-TARGET", customerId, OrderPaymentType.Debt);
        targetOrder.UpdateItems([new OrderItem(Guid.NewGuid().ToString(), 2, 10m)]);
        await OrderRepository.Add(targetOrder, CancellationToken.None);
        CompleteOrder(targetOrder);
        targetOrder.RecordPayment(5m, OrderPaymentMethod.Cash, DateTimeOffset.UtcNow);
        await OrderRepository.Update(targetOrder, CancellationToken.None);

        var wrongType = new Order("PAYMENT-COMBO-WRONG-TYPE", customerId, OrderPaymentType.Immediate);
        wrongType.UpdateItems([new OrderItem(Guid.NewGuid().ToString(), 2, 10m)]);
        await OrderRepository.Add(wrongType, CancellationToken.None);

        var wrongStatus = new Order("PAYMENT-COMBO-WRONG-STATUS", customerId, OrderPaymentType.Debt);
        wrongStatus.UpdateItems([new OrderItem(Guid.NewGuid().ToString(), 2, 10m)]);
        await OrderRepository.Add(wrongStatus, CancellationToken.None);

        var query = new ListOrdersQuery
        {
            Skip = 0,
            Length = 10,
            PaymentType = OrderPaymentType.Debt,
            PaymentStatus = OrderPaymentStatus.Partial
        };

        var result = await TestMediator.Send(query);

        result.ShouldBeSuccess();
        var page = result.Value!;
        page.Data.Should().ContainSingle(x => x.Id == targetOrder.Id);
    }
}
