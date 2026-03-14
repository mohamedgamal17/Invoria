using Invoria.BuildingBlocks.Domain.Primitives;

namespace Invoria.BuildingBlocks.Application.Abstractions.Pipeline;

public interface ICommandPipelineBehavior<TCommand>
{
    Task<Result> Handle(
        TCommand command,
        CancellationToken cancellationToken,
        Func<TCommand, CancellationToken, Task<Result>> next);
}

