using Autofac;
using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.Financial.Application.Payables.Commands.CreatePayable;
using Invoria.Financial.Application.Payables.Commands.SettlePayable;
using Invoria.Financial.Application.Payables.Queries.ListPayables;
using Invoria.Financial.Contracts.Receivables.Enums;
using Invoria.Financial.Domain.Payables;
using Invoria.Financial.Infrastructure.EntityFramework;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Financial.Application.Tests.Payables.Queries;

[TestFixture]
public class ListPayablesQueryHandlerTests : FinancialTestFixture
{
    private IMediator Mediator { get; }

    public ListPayablesQueryHandlerTests()
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
        var settlements = await db.Set<PayableSettlement>().ToListAsync();
        db.RemoveRange(settlements);
        var payables = await db.Set<Payable>().ToListAsync();
        db.RemoveRange(payables);
        await db.SaveChangesAsync();
    }

    private async Task<string> CreatePayableAsync(string partyId, string sourceId, decimal amount = 1000m)
    {
        var createResult = await Mediator.Send(new CreatePayableCommand(partyId, sourceId, amount));
        createResult.ShouldBeSuccess();

        return createResult.Value!.Id;
    }

    private async Task<List<Payable>> SetPayablesCreatedAtForOrderingAsync(
        IReadOnlyList<string> payableIds,
        DateTimeOffset baseTime)
    {
        var db = Scope.Resolve<FinancialDbContext>();
        var payables = await db.Set<Payable>()
            .Where(p => payableIds.Contains(p.Id))
            .ToListAsync();

        var property = typeof(Invoria.BuildingBlocks.Domain.Entities.AuditedAggregateRoot)
            .GetProperty(nameof(Invoria.BuildingBlocks.Domain.Entities.AuditedAggregateRoot.CreatedAt));

        for (var i = 0; i < payables.Count; i++)
        {
            var createdAt = baseTime.AddHours(i);
            property!.SetValue(payables[i], createdAt);
        }

        await db.SaveChangesAsync();

        return payables;
    }

    [Test]
    public async Task Should_return_empty_page_when_no_payables()
    {
        var query = new ListPayablesQuery { Skip = 0, Length = 10 };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        result.Value.Should().NotBeNull();
        result.Value!.Info.Skip.Should().Be(0);
        result.Value.Info.Length.Should().Be(10);
        result.Value.Info.TotalCount.Should().Be(0);
        result.Value.Data.Should().BeEmpty();
    }

    [Test]
    public async Task Should_return_paged_payables_ordered_by_created_at_descending()
    {
        var idA = await CreatePayableAsync("SUP-A", "PO-A");
        var idB = await CreatePayableAsync("SUP-B", "PO-B");
        var idC = await CreatePayableAsync("SUP-C", "PO-C");

        var payables = await SetPayablesCreatedAtForOrderingAsync(
            [idA, idB, idC],
            DateTimeOffset.UtcNow.AddDays(-10));

        var query = new ListPayablesQuery { Skip = 1, Length = 2 };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        var page = result.Value!;
        page.Info.Skip.Should().Be(1);
        page.Info.Length.Should().Be(2);
        page.Info.TotalCount.Should().Be(3);
        page.Data.Should().HaveCount(2);

        var orderedIds = payables
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
        var matchingPartyId = "SUP-" + Guid.NewGuid().ToString("N")[..8];
        var matchingId = await CreatePayableAsync(matchingPartyId, "PO-" + Guid.NewGuid().ToString("N")[..8]);
        await CreatePayableAsync("SUP-" + Guid.NewGuid().ToString("N")[..8], "PO-" + Guid.NewGuid().ToString("N")[..8]);

        var query = new ListPayablesQuery
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
        var matchingSourceId = "PO-" + Guid.NewGuid().ToString("N")[..8];
        var matchingId = await CreatePayableAsync("SUP-" + Guid.NewGuid().ToString("N")[..8], matchingSourceId);
        await CreatePayableAsync("SUP-" + Guid.NewGuid().ToString("N")[..8], "PO-" + Guid.NewGuid().ToString("N")[..8]);

        var query = new ListPayablesQuery
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
        var paidId = await CreatePayableAsync("SUP-" + Guid.NewGuid().ToString("N")[..8], "PO-" + Guid.NewGuid().ToString("N")[..8]);
        var paidResult = await Mediator.Send(new SettlePayableCommand(paidId, 1000m));
        paidResult.ShouldBeSuccess();

        await CreatePayableAsync("SUP-" + Guid.NewGuid().ToString("N")[..8], "PO-" + Guid.NewGuid().ToString("N")[..8]);

        var query = new ListPayablesQuery
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
        var paidId = await CreatePayableAsync("SUP-" + Guid.NewGuid().ToString("N")[..8], "PO-" + Guid.NewGuid().ToString("N")[..8]);
        var paidResult = await Mediator.Send(new SettlePayableCommand(paidId, 1000m));
        paidResult.ShouldBeSuccess();

        var partialId = await CreatePayableAsync("SUP-" + Guid.NewGuid().ToString("N")[..8], "PO-" + Guid.NewGuid().ToString("N")[..8]);
        var partialResult = await Mediator.Send(new SettlePayableCommand(partialId, 400m));
        partialResult.ShouldBeSuccess();

        var outstandingId = await CreatePayableAsync("SUP-" + Guid.NewGuid().ToString("N")[..8], "PO-" + Guid.NewGuid().ToString("N")[..8]);

        var query = new ListPayablesQuery
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
        var matchingPartyId = "SUP-" + Guid.NewGuid().ToString("N")[..8];
        var paidId = await CreatePayableAsync(matchingPartyId, "PO-" + Guid.NewGuid().ToString("N")[..8]);
        var paidResult = await Mediator.Send(new SettlePayableCommand(paidId, 1000m));
        paidResult.ShouldBeSuccess();

        await CreatePayableAsync(matchingPartyId, "PO-" + Guid.NewGuid().ToString("N")[..8]);

        var query = new ListPayablesQuery
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
