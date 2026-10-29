using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.Financial.Contracts.Payables.Dtos;

namespace Invoria.Financial.Application.Payables.Commands.SettlePayable;

public sealed class SettlePayableCommand : ICommand<PayableDto>
{
    public string PayableId { get; set; }
    public decimal Amount { get; set; }

    public SettlePayableCommand(string payableId, decimal amount)
    {
        PayableId = payableId;
        Amount = amount;
    }
}
