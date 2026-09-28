using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.Financial.Contracts.Receivables.Dtos;
using Invoria.Financial.Contracts.Receivables.Events;

namespace Invoria.Financial.Application.Receivables.Commands.CreateReceivable;

public sealed class CreateReceivableCommand : ICommand<ReceivableDto>
{
    public string PartyId { get; set; }
    public string SourceId { get; set; }
    public decimal Amount { get; set; }

    public CreateReceivableCommand(string partyId, string sourceId, decimal amount)
    {
        PartyId = partyId;
        SourceId = sourceId;
        Amount = amount;
    }

    public static CreateReceivableCommand FromEvent(CreateOrderReceivableIntegrationEvent message)
    {
        var command = new CreateReceivableCommand(
            partyId: message.CustomerId,
            sourceId: message.OrderId,
            amount: message.Amount);

        return command;
    }
}
