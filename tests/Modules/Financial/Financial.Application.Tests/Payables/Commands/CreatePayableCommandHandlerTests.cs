using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.Financial.Application.Payables.Commands.CreatePayable;
using Invoria.Financial.Application.Tests.Assertions;
using Invoria.Financial.Domain.Payables;
using Invoria.Financial.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Financial.Application.Tests.Payables.Commands;

[TestFixture]
public class CreatePayableCommandHandlerTests : FinancialTestFixture
{
    private IFinancialRepository<Payable> PayableRepository { get; }
    private IMediator Mediator { get; }

    public CreatePayableCommandHandlerTests()
    {
        PayableRepository = ServiceProvider.GetRequiredService<IFinancialRepository<Payable>>();
        Mediator = ServiceProvider.GetRequiredService<IMediator>();
    }

    [Test]
    public async Task Should_create_payable()
    {
        // Arrange
        var partyId = "SUP-" + Guid.NewGuid().ToString("N")[..8];
        var sourceId = "PO-" + Guid.NewGuid().ToString("N")[..8];
        var command = new CreatePayableCommand(
            partyId: partyId,
            sourceId: sourceId,
            amount: 2500.50m);

        // Act
        var result = await Mediator.Send(command);

        // Assert
        result.ShouldBeSuccess();

        var payable = await PayableRepository.SingleOrDefault(x => x.Id == result.Value!.Id);
        payable.Should().NotBeNull();

        payable!.AssertCreatePayableCommand(command);

        result.Value!.AssertPayableDto(payable);
    }
}
