using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.Inventory.Application.Batches.Queries.ListBatches;
using Invoria.Inventory.Application.Tests.Assertions;
using Invoria.Inventory.Domain;
using Invoria.Inventory.Domain.Batches;
using Invoria.Inventory.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Inventory.Application.Tests.Batches;

[TestFixture]
public class ListBatchesQueryHandlerTests : BatchTestFixture
{
    private IInventoryRepository<Batch> BatchRepository =>
        ServiceProvider.GetRequiredService<IInventoryRepository<Batch>>();

    protected override async Task BeforeAnyTestRunAsync()
    {
        await ClearBatchesAsync();
    }

    private async Task ClearBatchesAsync()
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        var batches = await db.Set<Batch>().ToListAsync();
        db.RemoveRange(batches);
        await db.SaveChangesAsync();
    }

    [Test]
    public async Task Should_return_empty_page_when_no_batches()
    {
        var query = new ListBatchesQuery { Skip = 0, Length = 10 };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        result.Value.Should().NotBeNull();
        result.Value!.AssertPagingDto(0, 10, 0, 0);
    }

    [Test]
    public async Task Should_return_paged_batches_ordered_by_created_at_descending()
    {
        var baseTime = DateTimeOffset.UtcNow.AddDays(-10);
        var batchA = CreateBatchWithCreatedAt("product-2", 10, 20m, baseTime);
        var batchB = CreateBatchWithCreatedAt("product-1", 5, 15m, baseTime.AddHours(1));
        var batchC = CreateBatchWithCreatedAt("product-3", 7, 30m, baseTime.AddHours(2));

        await BatchRepository.Add(batchA, CancellationToken.None);
        await BatchRepository.Add(batchB, CancellationToken.None);
        await BatchRepository.Add(batchC, CancellationToken.None);

        var query = new ListBatchesQuery { Skip = 1, Length = 2 };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        var page = result.Value!;
        page.AssertPagingDto(1, 2, 3, 2);

        var ordered = new[] { batchA, batchB, batchC }.OrderByDescending(x => x.CreatedAt).Select(x => x.Id).ToList();
        page.Data.Select(x => x.Id).Should().Equal(ordered.Skip(1).Take(2));
    }

    private static Batch CreateBatchWithCreatedAt(string productId, int quantity, decimal price, DateTimeOffset createdAt)
    {
        var batch = new Batch(productId, quantity, price);
        var property = typeof(Invoria.BuildingBlocks.Domain.Entities.AuditedAggregateRoot)
            .GetProperty(nameof(Invoria.BuildingBlocks.Domain.Entities.AuditedAggregateRoot.CreatedAt));
        property!.SetValue(batch, createdAt);
        return batch;
    }

    [Test]
    public async Task Should_filter_by_product_id_and_state()
    {
        var matching = await BatchRepository.Add(new Batch("product-match", 8, 12m), CancellationToken.None);

        var sameProductDifferentState = await BatchRepository.Add(new Batch("product-match", 3, 10m), CancellationToken.None);
        sameProductDifferentState.Disable();
        await BatchRepository.Update(sameProductDifferentState, CancellationToken.None);

        await BatchRepository.Add(new Batch("product-other", 8, 12m), CancellationToken.None);

        var query = new ListBatchesQuery
        {
            Skip = 0,
            Length = 10,
            ProductId = "product-match",
            State = BatchState.Active
        };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        var page = result.Value!;
        page.AssertPagingDto(0, 10, 1, 1);
        page.Data.Single().Id.Should().Be(matching.Id);
        page.Data.Single().ProductId.Should().Be("product-match");
    }
}
