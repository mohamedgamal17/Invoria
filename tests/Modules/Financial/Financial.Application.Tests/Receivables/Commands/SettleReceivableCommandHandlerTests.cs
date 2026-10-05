using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.Financial.Application.Receivables.Commands.CreateReceivable;
using Invoria.Financial.Application.Receivables.Commands.SettleReceivable;
using Invoria.Financial.Application.Tests.Assertions;
using Invoria.Financial.Contracts.Receivables.Enums;
using Invoria.Financial.Domain.Receivables;
using Invoria.Financial.Domain.Repositories;
using Invoria.Financial.Infrastructure.EntityFramework;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Financial.Application.Tests.Receivables.Commands;

[TestFixture]
public class SettleReceivableCommandHandlerTests : FinancialTestFixture
{
    private IFinancialRepository<Receivable> ReceivableRepository { get; }
    private IMediator Mediator { get; }

    public SettleReceivableCommandHandlerTests()
    {
        ReceivableRepository = ServiceProvider.GetRequiredService<IFinancialRepository<Receivable>>();
        Mediator = ServiceProvider.GetRequiredService<IMediator>();
    }

    [Test]
    public async Task Should_settle_partially()
    {
        // Arrange
        var createResult = await Mediator.Send(NewCreateCommand(1000m));
        createResult.ShouldBeSuccess();
        var receivableId = createResult.Value!.Id;
        var command = new SettleReceivableCommand(receivableId, 400m);

        // Act
        var result = await Mediator.Send(command);

        // Assert
        result.ShouldBeSuccess();
        result.Value!.OutstandingAmount.Should().Be(600m);
        result.Value.Status.Should().Be(FinancialObligationStatus.PartiallyPaid);

        var reloaded = await ReloadWithSettlementsAsync(receivableId);
        reloaded.OutstandingAmount.Should().Be(600m);
        reloaded.Status.Should().Be(FinancialObligationStatus.PartiallyPaid);

        var settlement = reloaded.Settlements.Should().ContainSingle().Subject;
        settlement.Amount.Should().Be(400m);
        settlement.ReceivableId.Should().Be(receivableId);
        settlement.SettledAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(5));

        result.Value.AssertReceivableDto(reloaded);
    }

    [Test]
    public async Task Should_settle_fully()
    {
        // Arrange
        var createResult = await Mediator.Send(NewCreateCommand(1000m));
        createResult.ShouldBeSuccess();
        var receivableId = createResult.Value!.Id;
        var command = new SettleReceivableCommand(receivableId, 1000m);

        // Act
        var result = await Mediator.Send(command);

        // Assert
        result.ShouldBeSuccess();
        result.Value!.OutstandingAmount.Should().Be(0m);
        result.Value.Status.Should().Be(FinancialObligationStatus.Paid);

        var reloaded = await ReloadWithSettlementsAsync(receivableId);
        reloaded.OutstandingAmount.Should().Be(0m);
        reloaded.Status.Should().Be(FinancialObligationStatus.Paid);

        var settlement = reloaded.Settlements.Should().ContainSingle().Subject;
        settlement.Amount.Should().Be(1000m);
        settlement.ReceivableId.Should().Be(receivableId);

        result.Value.AssertReceivableDto(reloaded);
    }

    [Test]
    public async Task Should_accumulate_multiple_settlements()
    {
        // Arrange
        var createResult = await Mediator.Send(NewCreateCommand(1000m));
        createResult.ShouldBeSuccess();
        var receivableId = createResult.Value!.Id;

        // Act
        var firstResult = await Mediator.Send(new SettleReceivableCommand(receivableId, 300m));
        var secondResult = await Mediator.Send(new SettleReceivableCommand(receivableId, 200m));

        // Assert
        firstResult.ShouldBeSuccess();
        secondResult.ShouldBeSuccess();
        secondResult.Value!.OutstandingAmount.Should().Be(500m);
        secondResult.Value.Status.Should().Be(FinancialObligationStatus.PartiallyPaid);

        var reloaded = await ReloadWithSettlementsAsync(receivableId);
        reloaded.OutstandingAmount.Should().Be(500m);
        reloaded.Status.Should().Be(FinancialObligationStatus.PartiallyPaid);
        reloaded.Settlements.Should().HaveCount(2);
        var totalSettled = reloaded.Settlements.Sum(s => s.Amount);
        totalSettled.Should().Be(500m);

        secondResult.Value.AssertReceivableDto(reloaded);
    }

    [Test]
    public async Task Should_fail_when_receivable_not_found()
    {
        // Arrange
        var missingId = Guid.NewGuid().ToString();
        var command = new SettleReceivableCommand(missingId, 100m);

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
        var receivableId = createResult.Value!.Id;
        var fullSettlement = await Mediator.Send(new SettleReceivableCommand(receivableId, 500m));
        fullSettlement.ShouldBeSuccess();

        // Act
        var result = await Mediator.Send(new SettleReceivableCommand(receivableId, 100m));

        // Assert
        result.ShouldBeFailure(typeof(BusinessLogicException));

        var reloaded = await ReloadWithSettlementsAsync(receivableId);
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
        var receivableId = createResult.Value!.Id;
        var partialSettlement = await Mediator.Send(new SettleReceivableCommand(receivableId, 600m));
        partialSettlement.ShouldBeSuccess();

        // Act
        var result = await Mediator.Send(new SettleReceivableCommand(receivableId, 500m));

        // Assert
        result.ShouldBeFailure(typeof(BusinessLogicException));

        var reloaded = await ReloadWithSettlementsAsync(receivableId);
        reloaded.OutstandingAmount.Should().Be(400m);
        reloaded.Status.Should().Be(FinancialObligationStatus.PartiallyPaid);
        reloaded.Settlements.Should().ContainSingle();
    }

    private static CreateReceivableCommand NewCreateCommand(decimal amount)
    {
        var partyId = "CUST-" + Guid.NewGuid().ToString("N")[..8];
        var sourceId = "INV-" + Guid.NewGuid().ToString("N")[..8];
        var command = new CreateReceivableCommand(partyId: partyId, sourceId: sourceId, amount: amount);

        return command;
    }

    private async Task<Receivable> ReloadWithSettlementsAsync(string receivableId)
    {
        await using var scope = ServiceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<FinancialDbContext>();
        var receivable = await db.Set<Receivable>().Include(r => r.Settlements).SingleAsync(r => r.Id == receivableId);

        return receivable;
    }
}
