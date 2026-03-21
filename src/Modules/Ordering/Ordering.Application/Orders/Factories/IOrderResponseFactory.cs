using Invoria.BuildingBlocks.Application.Factories;
using Invoria.Ordering.Contracts.Dtos;
using Invoria.Ordering.Domain.Orders;

namespace Invoria.Ordering.Application.Orders.Factories
{
    public interface IOrderResponseFactory : IResponseFactory<Order, OrderDto>
    {
    }
}
