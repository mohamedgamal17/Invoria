using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.Inventory.Application.Returns.Queries.ListReturns;
using Invoria.Inventory.Application.Tests.Assertions;
using Invoria.Inventory.Domain;
using Invoria.Inventory.Domain.Returns;
using Invoria.Inventory.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Invoria.Inventory.Application.Tests.Batches;

namespace Invoria.Inventory.Application.Tests.Returns;

[TestFixture]
public class ListReturnsQueryHandlerTests : BatchTestFixture
{
    private IInventoryRepository<Return> ReturnRepository =>
        ServiceProvider.GetRequiredService<IInventoryRepository<Return>>();

    protected override async Task BeforeAnyTestRunAsync()
    {
        await ClearReturnsAsync();
    }

    private async Task ClearReturnsAsync()
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        var returns = await db.Set<Return>().ToListAsync();
        db.RemoveRange(returns);
        await db.SaveChangesAsync();
    }

    [Test]
    public async Task Should_return_empty_page_when_no_returns()
    {
        var query = new ListReturnsQuery { Skip = 0, Length = 10 };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        result.Value.Should().NotBeNull();
        result.Value!.AssertPagingDto(0, 10, 0, 0);
    }

    [Test]
    public async Task Should_return_paged_returns_ordered_by_id_descending()
    {
        var returnA = ImmediateReturn.Create(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
        [
            ReturnLine.Create($"oi-{Guid.NewGuid():N}", Guid.NewGuid().ToString(), 2)
        ]);
        var returnB = ImmediateReturn.Create(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
        [
            ReturnLine.Create($"oi-{Guid.NewGuid():N}", Guid.NewGuid().ToString(), 3)
        ]);
        var returnC = ImmediateReturn.Create(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
        [
            ReturnLine.Create($"oi-{Guid.NewGuid():N}", Guid.NewGuid().ToString(), 1)
        ]);

        await ReturnRepository.Add(returnA);
        await ReturnRepository.Add(returnB);
        await ReturnRepository.Add(returnC);

        var query = new ListReturnsQuery { Skip = 1, Length = 2 };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        var page = result.Value!;
        page.AssertPagingDto(1, 2, 3, 2);

        var orderedIds = new[] { returnA.Id, returnB.Id, returnC.Id }
            .OrderByDescending(x => x)
            .ToList();
        page.Data.Select(x => x.Id).Should().Equal(orderedIds.Skip(1).Take(2));
    }

    [Test]
    public async Task Should_filter_by_return_type()
    {
        var matching = ImmediateReturn.Create(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
        [
            ReturnLine.Create($"oi-{Guid.NewGuid():N}", Guid.NewGuid().ToString(), 5)
        ]);

        await ReturnRepository.Add(matching);

        var query = new ListReturnsQuery
        {
            Skip = 0,
            Length = 10,
            Type = ReturnType.Immediate
        };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        var page = result.Value!;
        page.AssertPagingDto(0, 10, 1, 1);
        page.Data.Single().Id.Should().Be(matching.Id);
    }

    [Test]
    public async Task Should_return_all_types_when_type_is_null()
    {
        var returnEntity = ImmediateReturn.Create(
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
        [
            ReturnLine.Create($"oi-{Guid.NewGuid():N}", Guid.NewGuid().ToString(), 4)
        ]);

        await ReturnRepository.Add(returnEntity);

        var query = new ListReturnsQuery { Skip = 0, Length = 10 };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        var page = result.Value!;
        page.AssertPagingDto(0, 10, 1, 1);
        page.Data.Single().Id.Should().Be(returnEntity.Id);
    }
}
