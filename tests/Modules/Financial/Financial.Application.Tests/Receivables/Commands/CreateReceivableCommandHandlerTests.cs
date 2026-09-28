using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.Financial.Application.Receivables.Commands.CreateReceivable;
using Invoria.Financial.Application.Tests.Assertions;
using Invoria.Financial.Domain.Receivables;
using Invoria.Financial.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Financial.Application.Tests.Receivables.Commands;

[TestFixture]
public class CreateReceivableCommandHandlerTests : FinancialTestFixture
{
    private IFinancialRepository<Receivable> ReceivableRepository { get; }
    private IMediator Mediator { get; }

    public CreateReceivableCommandHandlerTests()
    {
        ReceivableRepository = ServiceProvider.GetRequiredService<IFinancialRepository<Receivable>>();
        Mediator = ServiceProvider.GetRequiredService<IMediator>();
    }

    [Test]
    public async Task Should_create_receivable()
    {
        // Arrange
        var partyId = "CUST-" + Guid.NewGuid().ToString("N")[..8];
        var sourceId = "INV-" + Guid.NewGuid().ToString("N")[..8];
        var command = new CreateReceivableCommand(
            partyId: partyId,
            sourceId: sourceId,
            amount: 1500.75m);

        // Act
        var result = await Mediator.Send(command);

        // Assert
        result.ShouldBeSuccess();

        var receivable = await ReceivableRepository.SingleOrDefault(x => x.Id == result.Value!.Id);
        receivable.Should().NotBeNull();

        receivable!.AssertCreateReceivableCommand(command);

        result.Value!.AssertReceivableDto(receivable);
    }
}
