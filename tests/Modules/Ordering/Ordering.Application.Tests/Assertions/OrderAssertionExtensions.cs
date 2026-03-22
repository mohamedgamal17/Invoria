using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.Catalog.Contracts.Dtos;
using Invoria.Ordering.Application.Orders.Commands.CreateOrder;
using Invoria.Ordering.Contracts.Dtos;

namespace Invoria.Ordering.Application.Tests.Assertions
{
    public static class OrderAssertionExtensions
    {
        public static void AssertOrderDto(this OrderDto dto, CreateOrderCommand command)
        {
            dto.Id.Should().NotBeNullOrWhiteSpace();
            dto.OrderNumber.Should().NotBeNullOrWhiteSpace();
            dto.CustomerId.Should().Be(command.CustomerId);
            dto.Items.Should().HaveCount(command.Items.Count);

            for (int i = 0; i < command.Items.Count; i++)
            {
                dto.Items[i].AssertOrderItemDto(command.Items[i], expectedProduct: null);
            }
        }

        public static void AssertOrderDto(
            this OrderDto dto,
            CreateOrderCommand command,
            Func<string, ProductDto?> resolveExpectedProduct)
        {
            dto.Id.Should().NotBeNullOrWhiteSpace();
            dto.OrderNumber.Should().NotBeNullOrWhiteSpace();
            dto.CustomerId.Should().Be(command.CustomerId);
            dto.Items.Should().HaveCount(command.Items.Count);

            for (int i = 0; i < command.Items.Count; i++)
            {
                var expected = resolveExpectedProduct(command.Items[i].ProductId);
                dto.Items[i].AssertOrderItemDto(command.Items[i], expected);
            }
        }

        public static void AssertPagingMetadata(
            this PagingDto<OrderDto> page,
            int expectedSkip,
            int expectedLength,
            long expectedTotalCount)
        {
            page.Info.Skip.Should().Be(expectedSkip);
            page.Info.Length.Should().Be(expectedLength);
            page.Info.TotalCount.Should().Be(expectedTotalCount);
        }

        public static void AssertPageDataCount(this PagingDto<OrderDto> page, int expectedCount)
        {
            page.Data.Count().Should().Be(expectedCount);
        }

        /// <summary>
        /// Asserts paging info and optionally the number of items in <see cref="PagingDto{T}.Data"/>.
        /// </summary>
        public static void AssertPagingDto(
            this PagingDto<OrderDto> page,
            int expectedSkip,
            int expectedLength,
            long expectedTotalCount,
            int? expectedDataCount = null)
        {
            page.AssertPagingMetadata(expectedSkip, expectedLength, expectedTotalCount);
            if (expectedDataCount.HasValue)
            {
                page.AssertPageDataCount(expectedDataCount.Value);
            }
        }

        public static void AssertOrderItemDto(
            this OrderItemDto dto,
            CreateOrderItemCommand commandItem,
            ProductDto? expectedProduct)
        {
            dto.ProductId.Should().Be(commandItem.ProductId);
            dto.Quantity.Should().Be(commandItem.Quantity);
            dto.Price.Should().Be(commandItem.Price);

            if (expectedProduct is null)
            {
                dto.Product.Should().BeNull();
            }
            else
            {
                dto.Product.Should().NotBeNull();
                dto.Product!.Id.Should().Be(expectedProduct.Id);
                dto.Product.Name.Should().Be(expectedProduct.Name);
                dto.Product.Code.Should().Be(expectedProduct.Code);
                dto.Product.Price.Should().Be(expectedProduct.Price);
            }
        }
    }
}
