using Invoria.BuildingBlocks.Application.Factories;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.Catalog.Contracts.Dtos;
using Invoria.Catalog.Contracts.Services;
using Invoria.CustomerManagement.Contracts.Dtos;
using Invoria.CustomerManagement.Contracts.Services;
using Invoria.Ordering.Contracts.Dtos;
using Invoria.Ordering.Contracts.Orders;
using Invoria.Ordering.Domain.Orders;

namespace Invoria.Ordering.Application.Orders.Factories
{
    public class OrderResponseFactory : ResponseFactory<Order, OrderDto>, IOrderResponseFactory
    {
        private readonly IProductService _productService;
        private readonly ICustomerService _customerService;

        public OrderResponseFactory(IProductService productService, ICustomerService customerService)
        {
            _productService = productService;
            _customerService = customerService;
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

            var customerIds = paging.Data
                .Select(o => o.CustomerId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            var customerById = await LoadCustomersByIdsAsync(customerIds, cancellationToken);
            var data = paging.Data.Select(o => MapToSummaryDto(o, customerById)).ToList();
            return new PagingDto<OrderDto>
            {
                Data = data,
                Info = paging.Info
            };
        }

        private OrderDto MapToSummaryDto(Order view, IReadOnlyDictionary<string, CustomerDto> customerById)
        {
            var dto = new OrderDto
            {
                Id = view.Id,
                OrderNumber = view.OrderNumber,
                CustomerId = view.CustomerId,
                Customer = customerById.GetValueOrDefault(view.CustomerId),
                Status = view.Status,
                FullfillmentStatus = view.FullfillmentStatus,
                Items = new List<OrderItemDto>()
            };

            MapAudited(view, dto);

            return dto;
        }

        public override async Task<OrderDto> PrepareDto(Order view)
        {
            var productIds = ExtractDistinctProductIds(view.Items);
            var productById = await LoadProductsByIdsAsync(productIds, CancellationToken.None);

            var customerIds = string.IsNullOrWhiteSpace(view.CustomerId)
                ? Array.Empty<string>()
                : new[] { view.CustomerId };
            var customerById = await LoadCustomersByIdsAsync(customerIds, CancellationToken.None);

            return MapToDto(view, productById, customerById);
        }

        public override async Task<List<OrderDto>> PrepareListDto(List<Order> views)
        {
            var productIds = views
                .SelectMany(o => o.Items)
                .Select(i => i.ProductId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            var customerIds = views
                .Select(o => o.CustomerId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            var productById = await LoadProductsByIdsAsync(productIds, CancellationToken.None);
            var customerById = await LoadCustomersByIdsAsync(customerIds, CancellationToken.None);

            return views.Select(view => MapToDto(view, productById, customerById)).ToList();
        }

        private OrderDto MapToDto(
            Order view,
            IReadOnlyDictionary<string, ProductDto> productById,
            IReadOnlyDictionary<string, CustomerDto> customerById)
        {
            var dto = new OrderDto
            {
                Id = view.Id,
                OrderNumber = view.OrderNumber,
                CustomerId = view.CustomerId,
                Customer = customerById.GetValueOrDefault(view.CustomerId),
                Status = view.Status,
                FullfillmentStatus = view.FullfillmentStatus,
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

        private async Task<Dictionary<string, CustomerDto>> LoadCustomersByIdsAsync(
            IReadOnlyCollection<string> ids,
            CancellationToken cancellationToken)
        {
            if (ids.Count == 0)
            {
                return new Dictionary<string, CustomerDto>();
            }

            var result = await _customerService.ListCustomersByIdsAsync(ids, cancellationToken);
            if (!result.IsSuccess || result.Value is null)
            {
                return new Dictionary<string, CustomerDto>();
            }

            return result.Value.ToDictionary(c => c.Id);
        }
    }
}
