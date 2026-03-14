using Invoria.BuildingBlocks.Domain.Primitives;

namespace Invoria.BuildingBlocks.Application.Abstractions.Pipeline;

public interface IQueryPipelineBehavior<TQuery, TResponse>
{
    Task<Result<TResponse>> Handle(
        TQuery query,
        CancellationToken cancellationToken,
        Func<TQuery, CancellationToken, Task<Result<TResponse>>> next);
}

