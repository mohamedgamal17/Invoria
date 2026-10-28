using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.Financial.Contracts.Payables.Dtos;
using Invoria.Financial.Contracts.Payables.Events;

namespace Invoria.Financial.Application.Payables.Commands.CreatePayable;

public sealed class CreatePayableCommand : ICommand<PayableDto>
{
    public string PartyId { get; set; }
    public string SourceId { get; set; }
    public decimal Amount { get; set; }

    public CreatePayableCommand(string partyId, string sourceId, decimal amount)
    {
        PartyId = partyId;
        SourceId = sourceId;
        Amount = amount;
    }

    public static CreatePayableCommand FromEvent(CreatePurchaseOrderPayableIntegrationEvent message)
    {
        var command = new CreatePayableCommand(
            partyId: message.SupplierId,
            sourceId: message.PurchaseOrderId,
            amount: message.Amount);

        return command;
    }
}
