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
            bool includeReturnItems = false,
            CancellationToken cancellationToken = default)
        {
            if (includeOrderItems)
            {
                var productIds = paging.Data
                    .SelectMany(o => ExtractDistinctProductIds(o.Items, o.FailureDetails))
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Distinct()
                    .ToList();

                var customerIds = paging.Data
                    .Select(o => o.CustomerId)
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Distinct()
                    .ToList();

                var productById = await LoadProductsByIdsAsync(productIds, cancellationToken);
                var customerById = await LoadCustomersByIdsAsync(customerIds, cancellationToken);
                var data = paging.Data
                    .Select(view => MapToDto(view, productById, customerById, includeReturnItems))
                    .ToList();

                return new PagingDto<OrderDto>
                {
                    Data = data,
                    Info = paging.Info
                };
            }

            var summaryCustomerIds = paging.Data
                .Select(o => o.CustomerId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            var summaryCustomerById = await LoadCustomersByIdsAsync(summaryCustomerIds, cancellationToken);

            if (includeReturnItems)
            {
                var productIds = paging.Data
                    .SelectMany(o => ExtractDistinctProductIds(o.Items, o.FailureDetails))
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Distinct()
                    .ToList();

                var productById = await LoadProductsByIdsAsync(productIds, cancellationToken);
                var dataWithReturns = paging.Data.Select(view =>
                {
                    var dto = MapToSummaryDto(view, summaryCustomerById, productById);
                    dto.ReturnItems = MapReturnItems(view, productById);
                    ApplyPricingScalars(view, dto);
                    return dto;
                }).ToList();

                return new PagingDto<OrderDto>
                {
                    Data = dataWithReturns,
                    Info = paging.Info
                };
            }

            var failureProductIds = paging.Data
                .SelectMany(o => o.FailureDetails)
                .Select(f => f.ItemId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();
            var failureProductById = await LoadProductsByIdsAsync(failureProductIds, cancellationToken);
            var summaryData = paging.Data
                .Select(o => MapToSummaryDto(o, summaryCustomerById, failureProductById))
                .ToList();

            return new PagingDto<OrderDto>
            {
                Data = summaryData,
                Info = paging.Info
            };
        }

        private OrderDto MapToSummaryDto(
            Order view,
            IReadOnlyDictionary<string, CustomerDto> customerById,
            IReadOnlyDictionary<string, ProductDto> productById)
        {
            var dto = new OrderDto
            {
                Id = view.Id,
                OrderNumber = view.OrderNumber,
                CustomerId = view.CustomerId,
                Customer = customerById.GetValueOrDefault(view.CustomerId),
                Status = view.Status,
                FullfillmentStatus = view.FullfillmentStatus,
                Payments = new List<OrderPaymentDto>(),
                Items = new List<OrderItemDto>(),
                StateTransitionHistory = view.StateTransitionHistory
                    .Select(transition => new OrderStateTransitionHistoryDto
                    {
                        FromStatus = transition.FromStatus,
                        ToStatus = transition.ToStatus,
                        FromFullfillmentStatus = transition.FromFullfillmentStatus,
                        ToFullfillmentStatus = transition.ToFullfillmentStatus,
                        ChangedAt = transition.ChangedAt,
                        Reason = transition.Reason
                    })
                    .ToList(),
                FailureDetails = view.FailureDetails
                    .Select(detail =>
                    {
                        var failureDto = new OrderFailureDetailsDto
                        {
                            Id = detail.Id,
                            ItemId = detail.ItemId,
                            ItemName = productById.GetValueOrDefault(detail.ItemId)?.Name,
                            QuantityRequested = detail.QuantityRequested,
                            QuantityAvailable = detail.QuantityAvailable,
                            Shortage = detail.Shortage
                        };
                        MapAudited(detail, failureDto);
                        return failureDto;
                    })
                    .ToList()
            };

            ApplyPaymentScalars(view, dto);
            MapAudited(view, dto);

            return dto;
        }

        public override async Task<OrderDto> PrepareDto(Order view)
        {
            var productIds = ExtractDistinctProductIds(view.Items, view.FailureDetails);
            var productById = await LoadProductsByIdsAsync(productIds, CancellationToken.None);

            var customerIds = string.IsNullOrWhiteSpace(view.CustomerId)
                ? Array.Empty<string>()
                : new[] { view.CustomerId };
            var customerById = await LoadCustomersByIdsAsync(customerIds, CancellationToken.None);

            return MapToDto(view, productById, customerById, includeReturnItems: true);
        }

        public override async Task<List<OrderDto>> PrepareListDto(List<Order> views)
        {
            var productIds = views
                .SelectMany(o => ExtractDistinctProductIds(o.Items, o.FailureDetails))
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

            return views
                .Select(view => MapToDto(view, productById, customerById, includeReturnItems: true))
                .ToList();
        }

        private OrderDto MapToDto(
            Order view,
            IReadOnlyDictionary<string, ProductDto> productById,
            IReadOnlyDictionary<string, CustomerDto> customerById,
            bool includeReturnItems)
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
                        Id = item.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = item.Price,
                        Product = productById.GetValueOrDefault(item.ProductId)
                    })
                    .ToList(),
                StateTransitionHistory = view.StateTransitionHistory
                    .Select(transition => new OrderStateTransitionHistoryDto
                    {
                        FromStatus = transition.FromStatus,
                        ToStatus = transition.ToStatus,
                        FromFullfillmentStatus = transition.FromFullfillmentStatus,
                        ToFullfillmentStatus = transition.ToFullfillmentStatus,
                        ChangedAt = transition.ChangedAt,
                        Reason = transition.Reason
                    })
                    .ToList(),
                FailureDetails = view.FailureDetails
                    .Select(detail =>
                    {
                        var dto = new OrderFailureDetailsDto
                        {
                            Id = detail.Id,
                            ItemId = detail.ItemId,
                            ItemName = productById.GetValueOrDefault(detail.ItemId)?.Name,
                            QuantityRequested = detail.QuantityRequested,
                            QuantityAvailable = detail.QuantityAvailable,
                            Shortage = detail.Shortage
                        };
                        MapAudited(detail, dto);
                        return dto;
                    })
                    .ToList(),
                Payments = MapPayments(view.Payments),
                ReturnItems = includeReturnItems
                    ? MapReturnItems(view, productById)
                    : new List<OrderReturnItemDto>(),
            };

            if (includeReturnItems)
            {
                ApplyPricingScalars(view, dto);
            }

            ApplyPaymentScalars(view, dto);
            MapAudited(view, dto);

            return dto;
        }

        private static List<OrderReturnItemDto> MapReturnItems(
            Order view,
            IReadOnlyDictionary<string, ProductDto> productById)
        {
            if (view.ReturnItems.Count == 0)
            {
                return new List<OrderReturnItemDto>();
            }

            var itemsById = view.Items.ToDictionary(i => i.Id);

            return view.ReturnItems
                .Select(returnItem =>
                {
                    var line = itemsById[returnItem.OrderItemId];
                    return new OrderReturnItemDto
                    {
                        OrderItemId = returnItem.OrderItemId,
                        Quantity = returnItem.Quantity,
                        ProductId = line.ProductId,
                        OrderedQuantity = line.Quantity,
                        UnitPrice = line.Price,
                        LineReturnTotal = line.Price * returnItem.Quantity,
                        Product = productById.GetValueOrDefault(line.ProductId),
                    };
                })
                .ToList();
        }

        private static void ApplyPricingScalars(Order view, OrderDto dto)
        {
            dto.TotalOrderAmount = view.TotalOrderAmount;
            dto.NetOfTotalOrderAmount = view.NetOfTotalOrderAmount;
            dto.ReturnsTotal = view.TotalOrderAmount - view.NetOfTotalOrderAmount;
        }

        private List<OrderPaymentDto> MapPayments(IReadOnlyCollection<OrderPayment> payments)
        {
            var list = new List<OrderPaymentDto>();
            foreach (var p in payments.OrderBy(x => x.PaidAt))
            {
                var paymentDto = new OrderPaymentDto
                {
                    Id = p.Id,
                    OrderId = p.OrderId,
                    PaidAmount = p.PaidAmount,
                    PaymentMethod = p.PaymentMethod,
                    PaidAt = p.PaidAt,
                };
                MapAudited(p, paymentDto);
                list.Add(paymentDto);
            }

            return list;
        }

        private static List<string> ExtractDistinctProductIds(
            IReadOnlyCollection<OrderItem> items,
            IReadOnlyCollection<OrderFailureDetails> failureDetails)
        {
            var itemProductIds = items
                .Select(i => i.ProductId)
                .Where(id => !string.IsNullOrWhiteSpace(id));

            var failedProductIds = failureDetails
                .Select(f => f.ItemId)
                .Where(id => !string.IsNullOrWhiteSpace(id));

            return itemProductIds
                .Concat(failedProductIds)
                .Distinct()
                .ToList();
        }

        private static void ApplyPaymentScalars(Order view, OrderDto dto)
        {
            dto.PaymentType = view.PaymentType;
            dto.AmountPaid = view.AmountPaid;
            dto.AmountOutstanding = view.AmountOutstanding;
            dto.PaymentStatus = view.PaymentStatus;
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
