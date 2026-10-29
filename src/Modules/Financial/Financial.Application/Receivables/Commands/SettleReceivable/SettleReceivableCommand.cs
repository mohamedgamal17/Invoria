using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.Financial.Contracts.Receivables.Dtos;

namespace Invoria.Financial.Application.Receivables.Commands.SettleReceivable;

public sealed class SettleReceivableCommand : ICommand<ReceivableDto>
{
    public string ReceivableId { get; set; }
    public decimal Amount { get; set; }

    public SettleReceivableCommand(string receivableId, decimal amount)
    {
        ReceivableId = receivableId;
        Amount = amount;
    }
}
