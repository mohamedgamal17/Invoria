using FluentAssertions;
using Invoria.Financial.Application.Receivables.Commands.CreateReceivable;
using Invoria.Financial.Contracts.Receivables.Dtos;
using Invoria.Financial.Contracts.Receivables.Enums;
using Invoria.Financial.Domain.Receivables;

namespace Invoria.Financial.Application.Tests.Assertions
{
    public static class ReceivableAssertionExtensions
    {
        public static void AssertCreateReceivableCommand(this Receivable receivable, CreateReceivableCommand command)
        {
            receivable.PartyId.Should().Be(command.PartyId);
            receivable.SourceId.Should().Be(command.SourceId);
            receivable.Amount.Should().Be(command.Amount);
            receivable.OutstandingAmount.Should().Be(command.Amount);
            receivable.Status.Should().Be(FinancialObligationStatus.Outstanding);
        }

        public static void AssertReceivableDto(this ReceivableDto dto, Receivable receivable)
        {
            dto.Id.Should().Be(receivable.Id);
            dto.PartyId.Should().Be(receivable.PartyId);
            dto.SourceId.Should().Be(receivable.SourceId);
            dto.Amount.Should().Be(receivable.Amount);
            dto.OutstandingAmount.Should().Be(receivable.OutstandingAmount);
            dto.Status.Should().Be(receivable.Status);
        }
    }
}
