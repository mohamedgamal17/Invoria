using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Inventory.Contracts.Returns.Events;

namespace Invoria.Inventory.Application.Returns.Commands.ProcessImmediateReturn;

public class ProcessImmediateReturnCommand : ICommand<Empty>
{
    public string ReturnId { get; init; } = string.Empty;

    public static ProcessImmediateReturnCommand FromEvent(ProcessImmediateReturnIntegrationEvent message) =>
        new() { ReturnId = message.ReturnId };
}
