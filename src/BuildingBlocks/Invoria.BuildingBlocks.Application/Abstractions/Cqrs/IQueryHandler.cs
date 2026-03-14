using Invoria.BuildingBlocks.Domain.Primitives;

namespace Invoria.BuildingBlocks.Application.Abstractions.Cqrs;

public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken = default);
}

