using Invoria.BuildingBlocks.Application.Factories;
using Invoria.Ordering.Contracts.Dtos;
using Invoria.Ordering.Domain.Orders;

namespace Invoria.Ordering.Application.Orders.Factories
{
    public class OrderResponseFactory : ResponseFactory<Order, OrderDto>, IOrderResponseFactory
    {
        public override Task<OrderDto> PrepareDto(Order view)
        {
            var dto = new OrderDto
            {
                Id = view.Id,
                OrderNumber = view.OrderNumber,
                CustomerId = view.CustomerId,
                Items = view.Items
                    .Select(item => new OrderItemDto
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = item.Price
                    })
                    .ToList()
            };

            MapAudited(view, dto);

            return Task.FromResult(dto);
        }
    }
}
