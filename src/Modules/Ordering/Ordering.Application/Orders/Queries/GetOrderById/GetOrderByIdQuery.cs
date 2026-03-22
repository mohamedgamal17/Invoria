using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.Ordering.Contracts.Dtos;

namespace Invoria.Ordering.Application.Orders.Queries.GetOrderById;

public class GetOrderByIdQuery : IQuery<OrderDto>
{
    public string Id { get; set; } = string.Empty;
}
