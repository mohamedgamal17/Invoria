using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.Inventory.Contracts.Returns.Dtos;

namespace Invoria.Inventory.Application.Returns.Commands.ApproveReturn;

public sealed class ApproveReturnCommand : ICommand<ReturnDto>
{
    public string ReturnId { get; }

    public ApproveReturnCommand(string returnId) => ReturnId = returnId;
}
