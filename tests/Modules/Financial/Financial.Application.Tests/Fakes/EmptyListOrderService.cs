using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Ordering.Contracts.Orders.Dtos;
using Invoria.Ordering.Contracts.Orders.Services;

namespace Invoria.Financial.Application.Tests.Fakes;

public sealed class EmptyListOrderService : IOrderService
{
    public Task<Result<PublicOrderDto>> GetOrderByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result.Failure<PublicOrderDto>(
            new NotFoundException($"Order with ID {id} not found")));
    }

    public Task<Result<IReadOnlyList<PublicOrderDto>>> ListOrdersByIdsAsync(
        IEnumerable<string> ids,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<PublicOrderDto> empty = Array.Empty<PublicOrderDto>();
        return Task.FromResult(Result.Success(empty));
    }
}
