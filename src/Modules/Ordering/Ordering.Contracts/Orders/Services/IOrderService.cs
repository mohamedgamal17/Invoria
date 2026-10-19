using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Ordering.Contracts.Orders.Dtos;

namespace Invoria.Ordering.Contracts.Orders.Services;

public interface IOrderService
{
    Task<Result<PublicOrderDto>> GetOrderByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<PublicOrderDto>>> ListOrdersByIdsAsync(
        IEnumerable<string> ids,
        CancellationToken cancellationToken = default);
}
