using Autofac;
using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.Financial.Application.Payables.Commands.CreatePayable;
using Invoria.Financial.Application.Payables.Commands.SettlePayable;
using Invoria.Financial.Application.Payables.Queries.GetPayableByPurchaseOrderId;
using Invoria.Financial.Contracts.Receivables.Enums;
using Invoria.Financial.Domain.Payables;
using Invoria.Financial.Infrastructure.EntityFramework;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Financial.Application.Tests.Payables.Queries;

[TestFixture]
public class GetPayableByPurchaseOrderIdQueryHandlerTests : FinancialTestFixture
{
    private IMediator Mediator { get; }

    public GetPayableByPurchaseOrderIdQueryHandlerTests()
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

    [Test]
    public async Task Should_return_payable_matching_purchase_order_id()
    {
        var purchaseOrderId = "PO-" + Guid.NewGuid().ToString("N")[..8];
        var partyId = "SUP-" + Guid.NewGuid().ToString("N")[..8];
        var payableId = await CreatePayableAsync(partyId, purchaseOrderId);

        await CreatePayableAsync(partyId, "PO-" + Guid.NewGuid().ToString("N")[..8]);

        var query = new GetPayableByPurchaseOrderIdQuery { PurchaseOrderId = purchaseOrderId };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        var dto = result.Value!;
        dto.Id.Should().Be(payableId);
        dto.PartyId.Should().Be(partyId);
        dto.SourceId.Should().Be(purchaseOrderId);
        dto.Amount.Should().Be(1000m);
    }

    [Test]
    public async Task Should_return_payable_with_settlements_for_purchase_order_id()
    {
        var purchaseOrderId = "PO-" + Guid.NewGuid().ToString("N")[..8];
        var partyId = "SUP-" + Guid.NewGuid().ToString("N")[..8];
        var payableId = await CreatePayableAsync(partyId, purchaseOrderId);

        var settleResult = await Mediator.Send(new SettlePayableCommand(payableId, 400m));
        settleResult.ShouldBeSuccess();

        var query = new GetPayableByPurchaseOrderIdQuery { PurchaseOrderId = purchaseOrderId };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        var dto = result.Value!;
        dto.Id.Should().Be(payableId);
        dto.SourceId.Should().Be(purchaseOrderId);
        dto.Status.Should().Be(FinancialObligationStatus.PartiallyPaid);
        dto.Settlements.Should().HaveCount(1);
        var settlement = dto.Settlements.Single();
        settlement.Amount.Should().Be(400m);
        settlement.PayableId.Should().Be(payableId);
    }

    [Test]
    public async Task Should_fail_with_not_found_when_no_payable_matches_purchase_order_id()
    {
        var missingPurchaseOrderId = "PO-" + Guid.NewGuid().ToString("N")[..8];
        await CreatePayableAsync("SUP-" + Guid.NewGuid().ToString("N")[..8], "PO-" + Guid.NewGuid().ToString("N")[..8]);

        var query = new GetPayableByPurchaseOrderIdQuery { PurchaseOrderId = missingPurchaseOrderId };

        var result = await Mediator.Send(query);

        result.ShouldBeFailure(typeof(NotFoundException));
    }
}
