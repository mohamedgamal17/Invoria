using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.Catalog.Contracts.Dtos;
using Invoria.CustomerManagement.Contracts.Dtos;
using Invoria.Ordering.Application.Orders.Commands.CreateOrder;
using Invoria.Ordering.Application.Orders.Commands.UpdateOrder;
using Invoria.Ordering.Contracts.Dtos;
using Invoria.Ordering.Domain.Orders;

namespace Invoria.Ordering.Application.Tests.Assertions
{
    public static class OrderAssertionExtensions
    {
        public static void AssertOrderDto(this OrderDto dto, Order order, CustomerDto? expectedCustomer = null)
        {
            dto.Id.Should().Be(order.Id);
            dto.OrderNumber.Should().Be(order.OrderNumber);
            dto.CustomerId.Should().Be(order.CustomerId);
            dto.AssertOrderCustomer(expectedCustomer);
            dto.Items.Should().HaveCount(order.Items.Count);
            foreach (var item in order.Items)
            {
                var line = dto.Items.Single(i => i.ProductId == item.ProductId);
                line.AssertOrderItemDto(item);
            }
        }

        public static void AssertOrderItemDto(this OrderItemDto dto, OrderItem item)
        {
            dto.ProductId.Should().Be(item.ProductId);
            dto.Quantity.Should().Be(item.Quantity);
            dto.Price.Should().Be(item.Price);
        }

        public static void AssertOrderDto(this OrderDto dto, CreateOrderCommand command, CustomerDto? expectedCustomer = null)
        {
            dto.Id.Should().NotBeNullOrWhiteSpace();
            dto.OrderNumber.Should().NotBeNullOrWhiteSpace();
            dto.CustomerId.Should().Be(command.CustomerId);
            dto.AssertOrderCustomer(expectedCustomer);
            dto.Items.Should().HaveCount(command.Items.Count);

            for (int i = 0; i < command.Items.Count; i++)
            {
                dto.Items[i].AssertOrderItemDto(command.Items[i], expectedProduct: null);
            }
        }

        public static void AssertOrderDto(this OrderDto dto, UpdateOrderCommand command)
        {
            dto.Id.Should().Be(command.Id);
            dto.Items.Should().HaveCount(command.Items.Count);

            for (int i = 0; i < command.Items.Count; i++)
            {
                dto.Items[i].AssertOrderItemDto(command.Items[i], expectedProduct: null);
            }
        }

        public static void AssertOrderDto(
            this OrderDto dto,
            CreateOrderCommand command,
            Func<string, ProductDto?> resolveExpectedProduct,
            CustomerDto? expectedCustomer = null)
        {
            dto.Id.Should().NotBeNullOrWhiteSpace();
            dto.OrderNumber.Should().NotBeNullOrWhiteSpace();
            dto.CustomerId.Should().Be(command.CustomerId);
            dto.AssertOrderCustomer(expectedCustomer);
            dto.Items.Should().HaveCount(command.Items.Count);

            for (int i = 0; i < command.Items.Count; i++)
            {
                var expected = resolveExpectedProduct(command.Items[i].ProductId);
                dto.Items[i].AssertOrderItemDto(command.Items[i], expected);
            }
        }

        public static void AssertOrderCustomer(this OrderDto dto, CustomerDto? expectedCustomer)
        {
            if (expectedCustomer is null)
            {
                dto.Customer.Should().BeNull();
            }
            else
            {
                dto.Customer.Should().NotBeNull();
                dto.Customer!.Id.Should().Be(expectedCustomer.Id);
                dto.Customer.Name.Should().Be(expectedCustomer.Name);
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
