using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.Financial.Application.Payables.Commands.CreatePayable;
using Invoria.Financial.Application.Payables.Commands.SettlePayable;
using Invoria.Financial.Application.Tests.Assertions;
using Invoria.Financial.Contracts.Receivables.Enums;
using Invoria.Financial.Domain.Payables;
using Invoria.Financial.Domain.Repositories;
using Invoria.Financial.Infrastructure.EntityFramework;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Financial.Application.Tests.Payables.Commands;

[TestFixture]
public class SettlePayableCommandHandlerTests : FinancialTestFixture
{
    private IFinancialRepository<Payable> PayableRepository { get; }
    private IMediator Mediator { get; }

    public SettlePayableCommandHandlerTests()
    {
        PayableRepository = ServiceProvider.GetRequiredService<IFinancialRepository<Payable>>();
        Mediator = ServiceProvider.GetRequiredService<IMediator>();
    }

    [Test]
    public async Task Should_settle_partially()
    {
        // Arrange
        var createResult = await Mediator.Send(NewCreateCommand(2000m));
        createResult.ShouldBeSuccess();
        var payableId = createResult.Value!.Id;
        var command = new SettlePayableCommand(payableId, 800m);

        // Act
        var result = await Mediator.Send(command);

        // Assert
        result.ShouldBeSuccess();
        result.Value!.OutstandingAmount.Should().Be(1200m);
        result.Value.Status.Should().Be(FinancialObligationStatus.PartiallyPaid);

        var reloaded = await ReloadWithSettlementsAsync(payableId);
        reloaded.OutstandingAmount.Should().Be(1200m);
        reloaded.Status.Should().Be(FinancialObligationStatus.PartiallyPaid);

        var settlement = reloaded.Settlements.Should().ContainSingle().Subject;
        settlement.Amount.Should().Be(800m);
        settlement.PayableId.Should().Be(payableId);
        settlement.SettledAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(5));

        result.Value.AssertPayableDto(reloaded);
    }

    [Test]
    public async Task Should_settle_fully()
    {
        // Arrange
        var createResult = await Mediator.Send(NewCreateCommand(2000m));
        createResult.ShouldBeSuccess();
        var payableId = createResult.Value!.Id;
        var command = new SettlePayableCommand(payableId, 2000m);

        // Act
        var result = await Mediator.Send(command);

        // Assert
        result.ShouldBeSuccess();
        result.Value!.OutstandingAmount.Should().Be(0m);
        result.Value.Status.Should().Be(FinancialObligationStatus.Paid);

        var reloaded = await ReloadWithSettlementsAsync(payableId);
        reloaded.OutstandingAmount.Should().Be(0m);
        reloaded.Status.Should().Be(FinancialObligationStatus.Paid);

        var settlement = reloaded.Settlements.Should().ContainSingle().Subject;
        settlement.Amount.Should().Be(2000m);
        settlement.PayableId.Should().Be(payableId);

        result.Value.AssertPayableDto(reloaded);
    }

    [Test]
    public async Task Should_accumulate_multiple_settlements()
    {
        // Arrange
        var createResult = await Mediator.Send(NewCreateCommand(2000m));
        createResult.ShouldBeSuccess();
        var payableId = createResult.Value!.Id;

        // Act
        var firstResult = await Mediator.Send(new SettlePayableCommand(payableId, 600m));
        var secondResult = await Mediator.Send(new SettlePayableCommand(payableId, 400m));

        // Assert
        firstResult.ShouldBeSuccess();
        secondResult.ShouldBeSuccess();
        secondResult.Value!.OutstandingAmount.Should().Be(1000m);
        secondResult.Value.Status.Should().Be(FinancialObligationStatus.PartiallyPaid);

        var reloaded = await ReloadWithSettlementsAsync(payableId);
        reloaded.OutstandingAmount.Should().Be(1000m);
        reloaded.Status.Should().Be(FinancialObligationStatus.PartiallyPaid);
        reloaded.Settlements.Should().HaveCount(2);
        var totalSettled = reloaded.Settlements.Sum(s => s.Amount);
        totalSettled.Should().Be(1000m);

        secondResult.Value.AssertPayableDto(reloaded);
    }

    [Test]
    public async Task Should_fail_when_payable_not_found()
    {
        // Arrange
        var missingId = Guid.NewGuid().ToString();
        var command = new SettlePayableCommand(missingId, 100m);

        // Act
        var result = await Mediator.Send(command);

        // Assert
        result.ShouldBeFailure(typeof(NotFoundException));
    }

    [Test]
    public async Task Should_fail_when_already_paid()
    {
        // Arrange
        var createResult = await Mediator.Send(NewCreateCommand(500m));
        createResult.ShouldBeSuccess();
        var payableId = createResult.Value!.Id;
        var fullSettlement = await Mediator.Send(new SettlePayableCommand(payableId, 500m));
        fullSettlement.ShouldBeSuccess();

        // Act
        var result = await Mediator.Send(new SettlePayableCommand(payableId, 100m));

        // Assert
        result.ShouldBeFailure(typeof(BusinessLogicException));

        var reloaded = await ReloadWithSettlementsAsync(payableId);
        reloaded.OutstandingAmount.Should().Be(0m);
        reloaded.Status.Should().Be(FinancialObligationStatus.Paid);
        reloaded.Settlements.Should().ContainSingle();
    }

    [Test]
    public async Task Should_fail_when_amount_exceeds_outstanding()
    {
        // Arrange
        var createResult = await Mediator.Send(NewCreateCommand(1000m));
        createResult.ShouldBeSuccess();
        var payableId = createResult.Value!.Id;
        var partialSettlement = await Mediator.Send(new SettlePayableCommand(payableId, 600m));
        partialSettlement.ShouldBeSuccess();

        // Act
        var result = await Mediator.Send(new SettlePayableCommand(payableId, 500m));

        // Assert
        result.ShouldBeFailure(typeof(BusinessLogicException));

        var reloaded = await ReloadWithSettlementsAsync(payableId);
        reloaded.OutstandingAmount.Should().Be(400m);
        reloaded.Status.Should().Be(FinancialObligationStatus.PartiallyPaid);
        reloaded.Settlements.Should().ContainSingle();
    }

    private static CreatePayableCommand NewCreateCommand(decimal amount)
    {
        var partyId = "SUP-" + Guid.NewGuid().ToString("N")[..8];
        var sourceId = "PO-" + Guid.NewGuid().ToString("N")[..8];
        var command = new CreatePayableCommand(partyId: partyId, sourceId: sourceId, amount: amount);

        return command;
    }

    private async Task<Payable> ReloadWithSettlementsAsync(string payableId)
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<FinancialDbContext>();
        var payable = await db.Set<Payable>().Include(p => p.Settlements).SingleAsync(p => p.Id == payableId);

        return payable;
    }
}
