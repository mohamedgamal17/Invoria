using FluentAssertions;
using Invoria.Financial.Application.Payables.Commands.CreatePayable;
using Invoria.Financial.Contracts.Payables.Dtos;
using Invoria.Financial.Contracts.Receivables.Enums;
using Invoria.Financial.Domain.Payables;

namespace Invoria.Financial.Application.Tests.Assertions
{
    public static class PayableAssertionExtensions
    {
        public static void AssertCreatePayableCommand(this Payable payable, CreatePayableCommand command)
        {
            payable.PartyId.Should().Be(command.PartyId);
            payable.SourceId.Should().Be(command.SourceId);
            payable.Amount.Should().Be(command.Amount);
            payable.OutstandingAmount.Should().Be(command.Amount);
            payable.Status.Should().Be(FinancialObligationStatus.Outstanding);
        }

        public static void AssertPayableDto(this PayableDto dto, Payable payable)
        {
            dto.Id.Should().Be(payable.Id);
            dto.PartyId.Should().Be(payable.PartyId);
            dto.SourceId.Should().Be(payable.SourceId);
            dto.Amount.Should().Be(payable.Amount);
            dto.OutstandingAmount.Should().Be(payable.OutstandingAmount);
            dto.Status.Should().Be(payable.Status);
        }
    }
}
