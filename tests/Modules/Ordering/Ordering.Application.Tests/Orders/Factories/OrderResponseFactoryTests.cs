using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.Catalog.Contracts.Services;
using Invoria.CustomerManagement.Contracts.Services;
using Invoria.Ordering.Contracts.Orders.Enums;
using Invoria.Ordering.Application.Orders.Factories;
using Invoria.Ordering.Tests.Fakes;
using Invoria.Ordering.Domain.Orders;
using Microsoft.Extensions.DependencyInjection;

using Entity = Invoria.BuildingBlocks.Domain.Entities.Entity<string>;

namespace Invoria.Ordering.Application.Tests.Orders.Factories;

[TestFixture]
public class OrderResponseFactoryTests : OrderingTestFixture
{
    [SetUp]
    public void ResetProductServiceCounters()
    {
        ProductCounter.ResetCounters();
        CustomerCounter.ResetCounters();
    }

    protected override void RegisterProductService(IServiceCollection services)
    {
        services.AddSingleton<CountingListProductsProductService>();
        services.AddSingleton<IProductService>(sp => sp.GetRequiredService<CountingListProductsProductService>());
    }

    protected override void RegisterCustomerService(IServiceCollection services)
    {
        services.AddSingleton<CountingListCustomersCustomerService>();
        services.AddSingleton<ICustomerService>(sp => sp.GetRequiredService<CountingListCustomersCustomerService>());
    }

    private CountingListProductsProductService ProductCounter =>
        ServiceProvider.GetRequiredService<CountingListProductsProductService>();

    private CountingListCustomersCustomerService CustomerCounter =>
        ServiceProvider.GetRequiredService<CountingListCustomersCustomerService>();

    private IOrderResponseFactory Factory =>
        ServiceProvider.GetRequiredService<IOrderResponseFactory>();

    [Test]
    public async Task PrepareListDto_should_call_ListProductsByIdsAsync_once_for_multiple_orders()
    {
        var pid1 = Guid.NewGuid().ToString();
        var pid2 = Guid.NewGuid().ToString();
        var customerId = Guid.NewGuid().ToString();

        var order1 = new Order("O1", customerId);
        order1.UpdateItems(new List<OrderItem> { new(pid1, 2, 10m), new(pid2, 1, 5m) });

        var order2 = new Order("O2", customerId);
        order2.UpdateItems(new List<OrderItem> { new(pid1, 1, 10m) });

        var order3 = new Order("O3", customerId);
        order3.UpdateItems(new List<OrderItem> { new(pid2, 3, 5m) });

        var orders = new List<Order> { order1, order2, order3 };

        var dtos = await Factory.PrepareListDto(orders);

        ProductCounter.ListProductsByIdsCallCount.Should().Be(1);
        CustomerCounter.ListCustomersByIdsCallCount.Should().Be(1);
        dtos.Should().HaveCount(3);

        dtos[0].Items.Should().HaveCount(2);
        dtos[0].Items[0].Product!.Id.Should().Be(pid1);
        dtos[0].Items[0].Product!.Name.Should().Be(SyntheticListProductService.NameForId(pid1));
        dtos[0].Items[1].Product!.Id.Should().Be(pid2);

        dtos[1].Items.Should().HaveCount(1);
        dtos[1].Items[0].Product!.Id.Should().Be(pid1);

        dtos[2].Items.Should().HaveCount(1);
        dtos[2].Items[0].Product!.Id.Should().Be(pid2);
    }

    [Test]
    public async Task PrepareListDto_empty_orders_should_not_call_ListProductsByIdsAsync()
    {
        await Factory.PrepareListDto(new List<Order>());

        ProductCounter.ListProductsByIdsCallCount.Should().Be(0);
        CustomerCounter.ListCustomersByIdsCallCount.Should().Be(0);
    }

    [Test]
    public async Task PreparePagingDto_should_use_single_batch_load()
    {
        var pid = Guid.NewGuid().ToString();
        var customerId = Guid.NewGuid().ToString();

        var order1 = new Order("A", customerId);
        order1.UpdateItems(new List<OrderItem> { new(pid, 1, 1m) });

        var order2 = new Order("B", customerId);
        order2.UpdateItems(new List<OrderItem> { new(pid, 2, 1m) });

        var paging = new PagingDto<Order>
        {
            Data = new List<Order> { order1, order2 },
            Info = new PagingInfoDto { Length = 2, Skip = 0, TotalCount = 2 }
        };

        var pagedDto = await Factory.PreparePagingDto(paging, includeOrderItems: true);

        ProductCounter.ListProductsByIdsCallCount.Should().Be(1);
        CustomerCounter.ListCustomersByIdsCallCount.Should().Be(1);
        pagedDto.Data.Should().HaveCount(2);
        pagedDto.Data.Should().OnlyContain(d => d.Items.All(i => i.Product!.Id == pid));
    }

    [Test]
    public async Task PreparePagingDto_summary_without_line_items_should_call_ListCustomersByIdsAsync_once()
    {
        var customerId = Guid.NewGuid().ToString();
        var order1 = new Order("A", customerId);
        var order2 = new Order("B", customerId);

        var paging = new PagingDto<Order>
        {
            Data = new List<Order> { order1, order2 },
            Info = new PagingInfoDto { Length = 2, Skip = 0, TotalCount = 2 }
        };

        var pagedDto = await Factory.PreparePagingDto(paging, includeOrderItems: false);

        CustomerCounter.ListCustomersByIdsCallCount.Should().Be(1);
        ProductCounter.ListProductsByIdsCallCount.Should().Be(0);
        pagedDto.Data.Should().HaveCount(2);
        pagedDto.Data.Should().OnlyContain(d => d.Customer!.Id == customerId);
        pagedDto.Data.Should().OnlyContain(d => d.Items.Count == 0);
    }

    [Test]
    public async Task PrepareDto_should_map_payment_aggregate_and_payment_lines_after_record_payment()
    {
        var customerId = Guid.NewGuid().ToString();
        var productId = Guid.NewGuid().ToString();
        var order = new Order("PAY-MAP", customerId, OrderPaymentType.Debt);
        typeof(Entity<string>).GetProperty(nameof(Entity<string>.Id))!.SetValue(order, $"paymap-{Guid.NewGuid():N}");

        order.UpdateItems(new List<OrderItem> { new(productId, 1, 100m) });
        order.Accept();
        order.Complete([]);
        order.RecordPayment(25m, OrderPaymentMethod.Cheque, DateTimeOffset.Parse("2026-06-01T10:00:00Z"));

        var dto = await Factory.PrepareDto(order);

        dto.PaymentType.Should().Be(OrderPaymentType.Debt);
        dto.AmountPaid.Should().Be(25m);
        dto.AmountOutstanding.Should().Be(75m);
        dto.PaymentStatus.Should().Be(OrderPaymentStatus.Partial);
        dto.Payments.Should().ContainSingle();
        dto.Payments[0].PaidAmount.Should().Be(25m);
        dto.Payments[0].PaymentMethod.Should().Be(OrderPaymentMethod.Cheque);
        dto.Payments[0].OrderId.Should().Be(order.Id);
    }

    [Test]
    public async Task PreparePagingDto_summary_with_returns_should_load_order_line_products()
    {
        var returnedProductId = Guid.NewGuid().ToString();
        var otherProductId = Guid.NewGuid().ToString();
        var customerId = Guid.NewGuid().ToString();
        var returnedLineId = "line-returned";
        var otherLineId = "line-other";

        var order = new Order("RET-SUM", customerId);
        order.UpdateItems(
        [
            new OrderItem(returnedProductId, 2, 10m),
            new OrderItem(otherProductId, 1, 5m)
        ]);
        SetEntityId(order, "order-ret-sum");
        SetEntityId(order.Items[0], returnedLineId);
        SetEntityId(order.Items[1], otherLineId);
        order.Accept();
        order.Complete([new OrderReturnItem(returnedLineId, 1)]);

        var paging = new PagingDto<Order>
        {
            Data = new List<Order> { order },
            Info = new PagingInfoDto { Length = 1, Skip = 0, TotalCount = 1 }
        };

        var paged = await Factory.PreparePagingDto(paging, includeOrderItems: false, includeReturnItems: true);

        ProductCounter.ListProductsByIdsCallCount.Should().Be(1);
        ProductCounter.LastRequestedProductIds.Should().Equal([returnedProductId, otherProductId]);

        var dto = paged.Data.Single();
        dto.Items.Should().BeEmpty();
        dto.ReturnItems.Should().ContainSingle();
        dto.ReturnItems[0].ProductId.Should().Be(returnedProductId);
        dto.ReturnItems[0].Product!.Id.Should().Be(returnedProductId);
        dto.ReturnItems[0].Product!.Name.Should().Be(SyntheticListProductService.NameForId(returnedProductId));
    }

    [Test]
    public async Task PreparePagingDto_with_line_items_without_returns_should_load_all_line_products()
    {
        var returnedProductId = Guid.NewGuid().ToString();
        var otherProductId = Guid.NewGuid().ToString();
        var customerId = Guid.NewGuid().ToString();

        var order = new Order("RET-NOLOAD", customerId);
        order.UpdateItems(
        [
            new OrderItem(returnedProductId, 2, 10m),
            new OrderItem(otherProductId, 1, 5m)
        ]);
        SetEntityId(order, "order-ret-noload");
        SetEntityId(order.Items[0], "line-returned");
        SetEntityId(order.Items[1], "line-other");
        order.Accept();
        order.Complete([new OrderReturnItem("line-returned", 1)]);

        var paging = new PagingDto<Order>
        {
            Data = new List<Order> { order },
            Info = new PagingInfoDto { Length = 1, Skip = 0, TotalCount = 1 }
        };

        var paged = await Factory.PreparePagingDto(
            paging,
            includeOrderItems: true,
            includeReturnItems: false);

        ProductCounter.ListProductsByIdsCallCount.Should().Be(1);
        ProductCounter.LastRequestedProductIds.Should().BeEquivalentTo([returnedProductId, otherProductId]);
        paged.Data.Single().ReturnItems.Should().BeEmpty();
    }

    [Test]
    public async Task PreparePublicOrderDto_should_map_every_public_field()
    {
        var customerId = Guid.NewGuid().ToString();
        var productId = Guid.NewGuid().ToString();
        var otherProductId = Guid.NewGuid().ToString();
        var allocationId = Guid.NewGuid().ToString();
        var returnId = Guid.NewGuid().ToString();
        var invoiceId = Guid.NewGuid().ToString();

        var order = new Order("PUBLIC-MAP", customerId, OrderPaymentType.Debt);
        typeof(Order).GetProperty(nameof(Order.AllocationId))!.SetValue(order, allocationId);
        typeof(Order).GetProperty(nameof(Order.ReturnId))!.SetValue(order, returnId);
        typeof(Order).GetProperty(nameof(Order.InvoiceId))!.SetValue(order, invoiceId);
        typeof(Order).GetProperty(nameof(Order.OrderAllocated))!.SetValue(order, true);
        SetEntityId(order, "public-map-order");
        order.UpdateItems(new List<OrderItem>
        {
            new(productId, 2, 10m),
            new(otherProductId, 1, 5m)
        });
        SetEntityId(order.Items[0], "public-line-returned");
        order.Accept();
        order.Complete([new OrderReturnItem("public-line-returned", 1)]);
        order.RecordPayment(5m, OrderPaymentMethod.Cash, DateTimeOffset.Parse("2026-06-02T10:00:00Z"));

        var dto = await Factory.PreparePublicOrderDto(order);

        dto.Id.Should().Be("public-map-order");
        dto.OrderNumber.Should().Be("PUBLIC-MAP");
        dto.CustomerId.Should().Be(customerId);
        dto.Status.Should().Be(OrderStatus.Completed);
        dto.PaymentType.Should().Be(OrderPaymentType.Debt);
        dto.SubtotalAmount.Should().Be(25m);
        dto.ReturnsAmount.Should().Be(10m);
        dto.NetOrderAmount.Should().Be(15m);
        dto.AmountOutstanding.Should().Be(10m);
        dto.AllocationId.Should().Be(allocationId);
        dto.ReturnId.Should().Be(returnId);
        dto.InvoiceId.Should().Be(invoiceId);
        dto.OrderAllocated.Should().BeTrue();
    }

    [Test]
    public async Task PreparePublicListDto_should_map_every_order()
    {
        var pid = Guid.NewGuid().ToString();
        var customerId = Guid.NewGuid().ToString();

        var order1 = new Order("PUB-1", customerId);
        order1.UpdateItems(new List<OrderItem> { new(pid, 2, 10m) });

        var order2 = new Order("PUB-2", customerId);
        order2.UpdateItems(new List<OrderItem> { new(pid, 1, 5m) });

        var orders = new List<Order> { order1, order2 };

        var dtos = await Factory.PreparePublicListDto(orders);

        dtos.Should().HaveCount(2);
        dtos[0].OrderNumber.Should().Be("PUB-1");
        dtos[0].CustomerId.Should().Be(customerId);
        dtos[0].SubtotalAmount.Should().Be(20m);
        dtos[0].ReturnsAmount.Should().Be(0m);
        dtos[0].NetOrderAmount.Should().Be(20m);
        dtos[0].AmountOutstanding.Should().Be(20m);
        dtos[0].OrderAllocated.Should().BeFalse();

        dtos[1].OrderNumber.Should().Be("PUB-2");
        dtos[1].SubtotalAmount.Should().Be(5m);
        dtos[1].NetOrderAmount.Should().Be(5m);
        dtos[1].AmountOutstanding.Should().Be(5m);
    }

    private static void SetEntityId(Entity entity, string id) =>
        typeof(Entity).GetProperty(nameof(Entity.Id))!.SetValue(entity, id);

    [Test]
    public async Task PreparePagingDto_summary_should_include_payment_scalars_with_empty_payment_lines()
    {
        var cid = Guid.NewGuid().ToString();
        var order = new Order("SUM-PAY", cid, OrderPaymentType.Debt);
        order.UpdateItems(new List<OrderItem> { new(Guid.NewGuid().ToString(), 2, 10m) });

        var paging = new PagingDto<Order>
        {
            Data = new List<Order> { order },
            Info = new PagingInfoDto { Length = 1, Skip = 0, TotalCount = 1 }
        };

        var paged = await Factory.PreparePagingDto(paging, includeOrderItems: false);

        var d = paged.Data.Single();
        d.Payments.Should().BeEmpty();
        d.PaymentType.Should().Be(OrderPaymentType.Debt);
        d.AmountPaid.Should().Be(0);
        d.AmountOutstanding.Should().Be(20m);
        d.PaymentStatus.Should().Be(OrderPaymentStatus.Unpaid);
    }

    [Test]
    public async Task PreparePagingDto_with_items_without_returns_omits_return_lines_and_pricing()
    {
        var pid = Guid.NewGuid().ToString();
        var customerId = Guid.NewGuid().ToString();
        var order = new Order("NO-RET", customerId);
        order.UpdateItems(new List<OrderItem> { new(pid, 2, 10m) });

        var paging = new PagingDto<Order>
        {
            Data = new List<Order> { order },
            Info = new PagingInfoDto { Length = 1, Skip = 0, TotalCount = 1 }
        };

        var paged = await Factory.PreparePagingDto(
            paging,
            includeOrderItems: true,
            includeReturnItems: false);

        var d = paged.Data.Single();
        d.Items.Should().ContainSingle();
        d.Items[0].Product!.Id.Should().Be(pid);
        d.ReturnItems.Should().BeEmpty();
        d.SubtotalAmount.Should().Be(0);
        d.NetOrderAmount.Should().Be(0);
        d.ReturnsAmount.Should().Be(0);
    }
}
