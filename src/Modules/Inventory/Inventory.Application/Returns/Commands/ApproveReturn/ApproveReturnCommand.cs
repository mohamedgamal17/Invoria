using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;

namespace Invoria.Inventory.Application.Returns.Commands.ApproveReturn;

public sealed class ApproveReturnCommand : ICommand<Empty>
{
    public string ReturnId { get; }

    public ApproveReturnCommand(string returnId) => ReturnId = returnId;
}
