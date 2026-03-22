using Invoria.BuildingBlocks.Application.Factories;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.Catalog.Contracts.Dtos;
using Invoria.Catalog.Contracts.Services;
using Invoria.Ordering.Contracts.Dtos;
using Invoria.Ordering.Domain.Orders;

namespace Invoria.Ordering.Application.Orders.Factories
{
    public class OrderResponseFactory : ResponseFactory<Order, OrderDto>, IOrderResponseFactory
    {
        private readonly IProductService _productService;

        public OrderResponseFactory(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<PagingDto<OrderDto>> PreparePagingDto(
            PagingDto<Order> paging,
            bool includeOrderItems,
            CancellationToken cancellationToken = default)
        {
            if (includeOrderItems)
            {
                return await base.PreparePagingDto(paging);
            }

            var data = paging.Data.Select(MapToSummaryDto).ToList();
            return new PagingDto<OrderDto>
            {
                Data = data,
                Info = paging.Info
            };
        }

        private OrderDto MapToSummaryDto(Order view)
        {
            var dto = new OrderDto
            {
                Id = view.Id,
                OrderNumber = view.OrderNumber,
                CustomerId = view.CustomerId,
                Items = new List<OrderItemDto>()
            };

            MapAudited(view, dto);

            return dto;
        }

        public override async Task<OrderDto> PrepareDto(Order view)
        {
            var ids = ExtractDistinctProductIds(view.Items);
            var productById = await LoadProductsByIdsAsync(ids, CancellationToken.None);
            return MapToDto(view, productById);
        }

        public override async Task<List<OrderDto>> PrepareListDto(List<Order> views)
        {
            var ids = views
                .SelectMany(o => o.Items)
                .Select(i => i.ProductId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            var productById = await LoadProductsByIdsAsync(ids, CancellationToken.None);
            return views.Select(view => MapToDto(view, productById)).ToList();
        }

        private OrderDto MapToDto(Order view, IReadOnlyDictionary<string, ProductDto> productById)
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
                        Price = item.Price,
                        Product = productById.GetValueOrDefault(item.ProductId)
                    })
                    .ToList()
            };

            MapAudited(view, dto);

            return dto;
        }

        private static List<string> ExtractDistinctProductIds(IReadOnlyCollection<OrderItem> items)
        {
            return items
                .Select(i => i.ProductId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();
        }

        private async Task<Dictionary<string, ProductDto>> LoadProductsByIdsAsync(
            IReadOnlyCollection<string> ids,
            CancellationToken cancellationToken)
        {
            if (ids.Count == 0)
            {
                return new Dictionary<string, ProductDto>();
            }

            var result = await _productService.ListProductsByIdsAsync(ids, cancellationToken);
            if (!result.IsSuccess || result.Value is null)
            {
                return new Dictionary<string, ProductDto>();
            }

            return result.Value.ToDictionary(p => p.Id);
        }
    }
}
