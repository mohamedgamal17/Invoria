using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.Catalog.Contracts.Services;
using Invoria.CustomerManagement.Contracts.Services;
using Invoria.Ordering.Application.Orders.Factories;
using Invoria.Ordering.Tests.Fakes;
using Invoria.Ordering.Domain.Orders;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Ordering.Application.Tests.Integration.Factories;

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

        var pagedDto = await Factory.PreparePagingDto(paging);

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
}
