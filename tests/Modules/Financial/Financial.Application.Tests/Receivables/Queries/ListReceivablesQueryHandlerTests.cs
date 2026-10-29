using Autofac;
using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.Financial.Application.Receivables.Commands.CreateReceivable;
using Invoria.Financial.Application.Receivables.Commands.SettleReceivable;
using Invoria.Financial.Application.Receivables.Queries.ListReceivables;
using Invoria.Financial.Contracts.Receivables.Enums;
using Invoria.Financial.Domain.Receivables;
using Invoria.Financial.Infrastructure.EntityFramework;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Financial.Application.Tests.Receivables.Queries;

[TestFixture]
public class ListReceivablesQueryHandlerTests : FinancialTestFixture
{
    private IMediator Mediator { get; }

    public ListReceivablesQueryHandlerTests()
    {
        Mediator = ServiceProvider.GetRequiredService<IMediator>();
    }

    protected override async Task BeforeAnyTestRunAsync()
    {
        await ClearDataAsync();
    }

    private async Task ClearDataAsync()
    {
        var db = Scope.Resolve<FinancialDbContext>();
        var settlements = await db.Set<ReceivableSettlement>().ToListAsync();
        db.RemoveRange(settlements);
        var receivables = await db.Set<Receivable>().ToListAsync();
        db.RemoveRange(receivables);
        await db.SaveChangesAsync();
    }

    private async Task<string> CreateReceivableAsync(string partyId, string sourceId, decimal amount = 1000m)
    {
        var createResult = await Mediator.Send(new CreateReceivableCommand(partyId, sourceId, amount));
        createResult.ShouldBeSuccess();

        return createResult.Value!.Id;
    }

    private async Task<List<Receivable>> SetReceivablesCreatedAtForOrderingAsync(
        IReadOnlyList<string> receivableIds,
        DateTimeOffset baseTime)
    {
        var db = Scope.Resolve<FinancialDbContext>();
        var receivables = await db.Set<Receivable>()
            .Where(r => receivableIds.Contains(r.Id))
            .ToListAsync();

        var property = typeof(Invoria.BuildingBlocks.Domain.Entities.AuditedAggregateRoot)
            .GetProperty(nameof(Invoria.BuildingBlocks.Domain.Entities.AuditedAggregateRoot.CreatedAt));

        for (var i = 0; i < receivables.Count; i++)
        {
            var createdAt = baseTime.AddHours(i);
            property!.SetValue(receivables[i], createdAt);
        }

        await db.SaveChangesAsync();

        return receivables;
    }

    [Test]
    public async Task Should_return_empty_page_when_no_receivables()
    {
        var query = new ListReceivablesQuery { Skip = 0, Length = 10 };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        result.Value.Should().NotBeNull();
        result.Value!.Info.Skip.Should().Be(0);
        result.Value.Info.Length.Should().Be(10);
        result.Value.Info.TotalCount.Should().Be(0);
        result.Value.Data.Should().BeEmpty();
    }

    [Test]
    public async Task Should_return_paged_receivables_ordered_by_created_at_descending()
    {
        var idA = await CreateReceivableAsync("PARTY-A", "SRC-A");
        var idB = await CreateReceivableAsync("PARTY-B", "SRC-B");
        var idC = await CreateReceivableAsync("PARTY-C", "SRC-C");

        var receivables = await SetReceivablesCreatedAtForOrderingAsync(
            [idA, idB, idC],
            DateTimeOffset.UtcNow.AddDays(-10));

        var query = new ListReceivablesQuery { Skip = 1, Length = 2 };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        var page = result.Value!;
        page.Info.Skip.Should().Be(1);
        page.Info.Length.Should().Be(2);
        page.Info.TotalCount.Should().Be(3);
        page.Data.Should().HaveCount(2);

        var orderedIds = receivables
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => x.Id)
            .ToList();
        var expectedIds = orderedIds.Skip(1).Take(2).ToList();
        var actualIds = page.Data.Select(x => x.Id).ToList();
        actualIds.Should().Equal(expectedIds);
    }

    [Test]
    public async Task Should_filter_by_party_id()
    {
        var matchingPartyId = "CUST-" + Guid.NewGuid().ToString("N")[..8];
        var matchingId = await CreateReceivableAsync(matchingPartyId, "SRC-" + Guid.NewGuid().ToString("N")[..8]);
        await CreateReceivableAsync("CUST-" + Guid.NewGuid().ToString("N")[..8], "SRC-" + Guid.NewGuid().ToString("N")[..8]);

        var query = new ListReceivablesQuery
        {
            Skip = 0,
            Length = 10,
            PartyId = matchingPartyId
        };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        var page = result.Value!;
        page.Info.TotalCount.Should().Be(1);
        var single = page.Data.Single();
        single.Id.Should().Be(matchingId);
        single.PartyId.Should().Be(matchingPartyId);
    }

    [Test]
    public async Task Should_filter_by_source_id()
    {
        var matchingSourceId = "SRC-" + Guid.NewGuid().ToString("N")[..8];
        var matchingId = await CreateReceivableAsync("CUST-" + Guid.NewGuid().ToString("N")[..8], matchingSourceId);
        await CreateReceivableAsync("CUST-" + Guid.NewGuid().ToString("N")[..8], "SRC-" + Guid.NewGuid().ToString("N")[..8]);

        var query = new ListReceivablesQuery
        {
            Skip = 0,
            Length = 10,
            SourceId = matchingSourceId
        };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        var page = result.Value!;
        page.Info.TotalCount.Should().Be(1);
        var single = page.Data.Single();
        single.Id.Should().Be(matchingId);
        single.SourceId.Should().Be(matchingSourceId);
    }

    [Test]
    public async Task Should_filter_by_is_paid_true()
    {
        var paidId = await CreateReceivableAsync("CUST-" + Guid.NewGuid().ToString("N")[..8], "SRC-" + Guid.NewGuid().ToString("N")[..8]);
        var paidResult = await Mediator.Send(new SettleReceivableCommand(paidId, 1000m));
        paidResult.ShouldBeSuccess();

        await CreateReceivableAsync("CUST-" + Guid.NewGuid().ToString("N")[..8], "SRC-" + Guid.NewGuid().ToString("N")[..8]);

        var query = new ListReceivablesQuery
        {
            Skip = 0,
            Length = 10,
            IsPaid = true
        };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        var page = result.Value!;
        page.Info.TotalCount.Should().Be(1);
        var single = page.Data.Single();
        single.Id.Should().Be(paidId);
        single.Status.Should().Be(FinancialObligationStatus.Paid);
    }

    [Test]
    public async Task Should_filter_by_is_paid_false_including_partially_paid()
    {
        var paidId = await CreateReceivableAsync("CUST-" + Guid.NewGuid().ToString("N")[..8], "SRC-" + Guid.NewGuid().ToString("N")[..8]);
        var paidResult = await Mediator.Send(new SettleReceivableCommand(paidId, 1000m));
        paidResult.ShouldBeSuccess();

        var partialId = await CreateReceivableAsync("CUST-" + Guid.NewGuid().ToString("N")[..8], "SRC-" + Guid.NewGuid().ToString("N")[..8]);
        var partialResult = await Mediator.Send(new SettleReceivableCommand(partialId, 400m));
        partialResult.ShouldBeSuccess();

        var outstandingId = await CreateReceivableAsync("CUST-" + Guid.NewGuid().ToString("N")[..8], "SRC-" + Guid.NewGuid().ToString("N")[..8]);

        var query = new ListReceivablesQuery
        {
            Skip = 0,
            Length = 10,
            IsPaid = false
        };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        var page = result.Value!;
        page.Info.TotalCount.Should().Be(2);
        var pageIds = page.Data.Select(x => x.Id).ToList();
        pageIds.Should().Contain(partialId);
        pageIds.Should().Contain(outstandingId);
        pageIds.Should().NotContain(paidId);
        page.Data.Should().OnlyContain(x => x.Status != FinancialObligationStatus.Paid);
    }

    [Test]
    public async Task Should_filter_by_party_id_and_is_paid_combined()
    {
        var matchingPartyId = "CUST-" + Guid.NewGuid().ToString("N")[..8];
        var paidId = await CreateReceivableAsync(matchingPartyId, "SRC-" + Guid.NewGuid().ToString("N")[..8]);
        var paidResult = await Mediator.Send(new SettleReceivableCommand(paidId, 1000m));
        paidResult.ShouldBeSuccess();

        await CreateReceivableAsync(matchingPartyId, "SRC-" + Guid.NewGuid().ToString("N")[..8]);

        var query = new ListReceivablesQuery
        {
            Skip = 0,
            Length = 10,
            PartyId = matchingPartyId,
            IsPaid = true
        };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        var page = result.Value!;
        page.Info.TotalCount.Should().Be(1);
        var single = page.Data.Single();
        single.Id.Should().Be(paidId);
        single.PartyId.Should().Be(matchingPartyId);
        single.Status.Should().Be(FinancialObligationStatus.Paid);
    }
}
