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
    public async Task Should_return_paged_batches_ordered_by_id_descending()
    {
        var batchA = await BatchRepository.Add(new Batch("product-2", 10, 20m), CancellationToken.None);
        var batchB = await BatchRepository.Add(new Batch("product-1", 5, 15m), CancellationToken.None);
        var batchC = await BatchRepository.Add(new Batch("product-3", 7, 30m), CancellationToken.None);

        var query = new ListBatchesQuery { Skip = 1, Length = 2 };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        var page = result.Value!;
        page.AssertPagingDto(1, 2, 3, 2);

        var orderedIds = new[] { batchA.Id, batchB.Id, batchC.Id }.OrderByDescending(x => x).ToList();
        page.Data.Select(x => x.Id).Should().Equal(orderedIds.Skip(1).Take(2));
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
