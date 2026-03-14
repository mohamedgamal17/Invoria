using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;

namespace Invoria.Modules.Products.Application.Ping;

public sealed record PingCommand(string Message) : ICommand<string>;

public sealed class PingCommandHandler
    : ICommandHandler<PingCommand, string>
{
    public Task<Result<string>> Handle(PingCommand command, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<string>.Success(command.Message));
    }
}
