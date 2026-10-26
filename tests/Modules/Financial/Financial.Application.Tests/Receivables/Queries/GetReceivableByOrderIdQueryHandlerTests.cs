using Autofac;
using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.Financial.Application.Receivables.Commands.CreateReceivable;
using Invoria.Financial.Application.Receivables.Commands.SettleReceivable;
using Invoria.Financial.Application.Receivables.Queries.GetReceivableByOrderId;
using Invoria.Financial.Contracts.Receivables.Enums;
using Invoria.Financial.Domain.Receivables;
using Invoria.Financial.Infrastructure.EntityFramework;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Financial.Application.Tests.Receivables.Queries;

[TestFixture]
public class GetReceivableByOrderIdQueryHandlerTests : FinancialTestFixture
{
    private IMediator Mediator { get; }

    public GetReceivableByOrderIdQueryHandlerTests()
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

    [Test]
    public async Task Should_return_receivable_matching_order_id()
    {
        var orderId = "ORD-" + Guid.NewGuid().ToString("N")[..8];
        var partyId = "CUST-" + Guid.NewGuid().ToString("N")[..8];
        var receivableId = await CreateReceivableAsync(partyId, orderId);

        await CreateReceivableAsync(partyId, "ORD-" + Guid.NewGuid().ToString("N")[..8]);

        var query = new GetReceivableByOrderIdQuery { OrderId = orderId };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        var dto = result.Value!;
        dto.Id.Should().Be(receivableId);
        dto.PartyId.Should().Be(partyId);
        dto.SourceId.Should().Be(orderId);
        dto.Amount.Should().Be(1000m);
    }

    [Test]
    public async Task Should_return_receivable_with_settlements_for_order_id()
    {
        var orderId = "ORD-" + Guid.NewGuid().ToString("N")[..8];
        var partyId = "CUST-" + Guid.NewGuid().ToString("N")[..8];
        var receivableId = await CreateReceivableAsync(partyId, orderId);

        var settleResult = await Mediator.Send(new SettleReceivableCommand(receivableId, 400m));
        settleResult.ShouldBeSuccess();

        var query = new GetReceivableByOrderIdQuery { OrderId = orderId };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        var dto = result.Value!;
        dto.Id.Should().Be(receivableId);
        dto.SourceId.Should().Be(orderId);
        dto.Status.Should().Be(FinancialObligationStatus.PartiallyPaid);
        dto.Settlements.Should().HaveCount(1);
        var settlement = dto.Settlements.Single();
        settlement.Amount.Should().Be(400m);
        settlement.ReceivableId.Should().Be(receivableId);
    }

    [Test]
    public async Task Should_fail_with_not_found_when_no_receivable_matches_order_id()
    {
        var missingOrderId = "ORD-" + Guid.NewGuid().ToString("N")[..8];
        await CreateReceivableAsync("CUST-" + Guid.NewGuid().ToString("N")[..8], "ORD-" + Guid.NewGuid().ToString("N")[..8]);

        var query = new GetReceivableByOrderIdQuery { OrderId = missingOrderId };

        var result = await Mediator.Send(query);

        result.ShouldBeFailure(typeof(NotFoundException));
    }
}
