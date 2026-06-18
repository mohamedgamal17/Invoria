using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;

namespace Invoria.Ordering.Application.Orders.Commands.RecordOrderReturn;

public sealed class RecordOrderReturnCommand : ICommand<Empty>
{
    public RecordOrderReturnCommand(string orderId, string returnId)
    {
        OrderId = orderId;
        ReturnId = returnId;
    }

    public string OrderId { get; }

    public string ReturnId { get; }
}
