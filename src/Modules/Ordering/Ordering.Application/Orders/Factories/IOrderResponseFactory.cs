using Invoria.BuildingBlocks.Application.Factories;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.Ordering.Contracts.Orders.Dtos;
using Invoria.Ordering.Domain.Orders;

namespace Invoria.Ordering.Application.Orders.Factories
{
    public interface IOrderResponseFactory : IResponseFactory<Order, OrderDto>
    {
        Task<PagingDto<OrderDto>> PreparePagingDto(
            PagingDto<Order> paging,
            bool includeOrderItems,
            bool includeReturnItems = false,
            CancellationToken cancellationToken = default);

        Task<PublicOrderDto> PreparePublicOrderDto(
            Order order,
            CancellationToken cancellationToken = default);

        Task<List<PublicOrderDto>> PreparePublicListDto(
            List<Order> orders,
            CancellationToken cancellationToken = default);
    }
}
