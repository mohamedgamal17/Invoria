using FluentAssertions;
using Invoria.Inventory.Application.Stock;
using Invoria.Inventory.Application.Tests.Batches;
using Invoria.Inventory.Domain;
using Invoria.Inventory.Domain.Batches;
using Invoria.Inventory.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Inventory.Application.Tests.Stock;

[TestFixture]
public class ProductStockServiceTests : BatchTestFixture
{
    private IProductStockService StockService => ServiceProvider.GetRequiredService<IProductStockService>();

    private IInventoryRepository<Batch> BatchRepository =>
        ServiceProvider.GetRequiredService<IInventoryRepository<Batch>>();

    private IInventoryRepository<BatchAllocation> AllocationRepository =>
        ServiceProvider.GetRequiredService<IInventoryRepository<BatchAllocation>>();

    protected override async Task BeforeAnyTestRunAsync()
    {
        await ClearInventoryAsync();
    }

    private async Task ClearInventoryAsync()
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        var allocations = await db.Set<BatchAllocation>().ToListAsync();
        db.RemoveRange(allocations);
        var batches = await db.Set<Batch>().ToListAsync();
        db.RemoveRange(batches);
        await db.SaveChangesAsync();
    }

    [Test]
    public async Task GetProductStock_sums_quantity_and_reserved_across_active_batches()
    {
        await BatchRepository.Add(new Batch("p1", 5, 10m), CancellationToken.None);
        await BatchRepository.Add(new Batch("p1", 7, 12m), CancellationToken.None);

        var batch = new Batch("p1", 10, 8m);
        await BatchRepository.Add(batch, CancellationToken.None);
        var tracked = (await BatchRepository.SingleOrDefault(b => b.Id == batch.Id, CancellationToken.None))!;
        var allocation = tracked.AllocateForOrder("oi-1", 3, DateTimeOffset.UtcNow);
        await BatchRepository.Update(tracked, CancellationToken.None);
        await AllocationRepository.Add(allocation, CancellationToken.None);

        var stock = await StockService.GetProductStock("p1");

        stock.ActualQuantity.Should().Be(5 + 7 + 7);
        stock.ReservedQuantity.Should().Be(3);
    }

    [Test]
    public async Task GetListProductsStock_returns_per_product_totals_in_one_query()
    {
        await BatchRepository.Add(new Batch("a", 2, 10m), CancellationToken.None);
        await BatchRepository.Add(new Batch("a", 3, 10m), CancellationToken.None);
        await BatchRepository.Add(new Batch("b", 11, 10m), CancellationToken.None);

        var map = await StockService.GetListProductsStock(new[] { "b", "a" });

        map.Should().HaveCount(2);
        map["a"].ActualQuantity.Should().Be(5);
        map["a"].ReservedQuantity.Should().Be(0);
        map["b"].ActualQuantity.Should().Be(11);
        map["b"].ReservedQuantity.Should().Be(0);
    }

    [Test]
    public async Task GetListProductsStock_excludes_disabled_batches()
    {
        await BatchRepository.Add(new Batch("p-x", 4, 10m), CancellationToken.None);
        var toDisable = await BatchRepository.Add(new Batch("p-x", 9, 10m), CancellationToken.None);
        toDisable.Disable();
        await BatchRepository.Update(toDisable, CancellationToken.None);

        var map = await StockService.GetListProductsStock(new[] { "p-x" });

        map["p-x"].ActualQuantity.Should().Be(4);
        map["p-x"].ReservedQuantity.Should().Be(0);
    }

    [Test]
    public async Task GetListProductsStock_returns_empty_when_no_ids()
    {
        var map = await StockService.GetListProductsStock(Array.Empty<string>());

        map.Should().BeEmpty();
    }

    [Test]
    public async Task GetListProductsStock_returns_zero_stock_for_unknown_product()
    {
        var map = await StockService.GetListProductsStock(new[] { "no-such-product" });

        map.Should().ContainKey("no-such-product");
        map["no-such-product"].ActualQuantity.Should().Be(0);
        map["no-such-product"].ReservedQuantity.Should().Be(0);
    }

    [Test]
    public async Task GetListProductsStock_deduplicates_product_ids()
    {
        await BatchRepository.Add(new Batch("p-dup", 6, 10m), CancellationToken.None);

        var map = await StockService.GetListProductsStock(new[] { "p-dup", " p-dup ", "p-dup" });

        map.Should().HaveCount(1);
        map["p-dup"].ActualQuantity.Should().Be(6);
    }
}
