using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.Catalog.Contracts.Dtos;
using Invoria.CustomerManagement.Contracts.Dtos;
using Invoria.Ordering.Application.Orders.Commands.CreateOrder;
using Invoria.Ordering.Application.Orders.Commands.UpdateOrder;
using Invoria.Ordering.Contracts.Dtos;
using Invoria.Ordering.Contracts.Orders;
using Invoria.Ordering.Domain.Orders;

namespace Invoria.Ordering.Application.Tests.Assertions
{
    public static class OrderAssertionExtensions
    {
        private static decimal SumCreateOrderLines(IEnumerable<CreateOrderItemCommand> items) =>
            items.Sum(i => i.Quantity * i.Price);

        public static void AssertOrderDto(this OrderDto dto, Order order, CustomerDto? expectedCustomer = null)
        {
            dto.Id.Should().Be(order.Id);
            dto.OrderNumber.Should().Be(order.OrderNumber);
            dto.CustomerId.Should().Be(order.CustomerId);
            dto.Status.Should().Be(order.Status);
            dto.PaymentType.Should().Be(order.PaymentType);
            dto.AmountPaid.Should().Be(order.AmountPaid);
            dto.AmountOutstanding.Should().Be(order.AmountOutstanding);
            dto.PaymentStatus.Should().Be(order.PaymentStatus);
            dto.Payments.Should().HaveCount(order.Payments.Count);
            dto.AssertOrderCustomer(expectedCustomer);
            dto.AssertOrderPricing(order);
            dto.ReturnItems.Should().HaveCount(order.ReturnItems.Count);
            dto.Items.Should().HaveCount(order.Items.Count);
            foreach (var item in order.Items)
            {
                var line = dto.Items.Single(i => i.ProductId == item.ProductId);
                line.AssertOrderItemDto(item);
            }

            var itemsById = order.Items.ToDictionary(i => i.Id);
            foreach (var returnItem in order.ReturnItems)
            {
                var line = itemsById[returnItem.OrderItemId];
                var returnDto = dto.ReturnItems.Single(r => r.OrderItemId == returnItem.OrderItemId);
                returnDto.Quantity.Should().Be(returnItem.Quantity);
                returnDto.ProductId.Should().Be(line.ProductId);
                returnDto.OrderedQuantity.Should().Be(line.Quantity);
                returnDto.UnitPrice.Should().Be(line.Price);
                returnDto.LineReturnTotal.Should().Be(line.Price * returnItem.Quantity);
            }

            var sortedPayments = order.Payments.OrderBy(p => p.PaidAt).ToList();
            for (var i = 0; i < sortedPayments.Count; i++)
            {
                var pm = dto.Payments.OrderBy(p => p.PaidAt).ElementAt(i);
                pm.OrderId.Should().Be(sortedPayments[i].OrderId);
                pm.PaidAmount.Should().Be(sortedPayments[i].PaidAmount);
                pm.PaymentMethod.Should().Be(sortedPayments[i].PaymentMethod);
                pm.PaidAt.Should().Be(sortedPayments[i].PaidAt);
                pm.Id.Should().Be(sortedPayments[i].Id);
            }
        }

        public static void AssertOrderItemDto(this OrderItemDto dto, OrderItem item)
        {
            dto.Id.Should().Be(item.Id);
            dto.ProductId.Should().Be(item.ProductId);
            dto.Quantity.Should().Be(item.Quantity);
            dto.Price.Should().Be(item.Price);
        }

        public static void AssertOrderDto(this OrderDto dto, CreateOrderCommand command, CustomerDto? expectedCustomer = null)
        {
            dto.Id.Should().NotBeNullOrWhiteSpace();
            dto.OrderNumber.Should().NotBeNullOrWhiteSpace();
            dto.CustomerId.Should().Be(command.CustomerId);
            dto.Status.Should().Be(OrderStatus.Pending);
            dto.PaymentType.Should().Be(command.PaymentType);
            dto.AmountPaid.Should().Be(0);
            dto.Payments.Should().BeEmpty();
            var total = SumCreateOrderLines(command.Items);
            dto.AmountOutstanding.Should().Be(total);
            dto.PaymentStatus.Should().Be(OrderPaymentStatus.Unpaid);
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

            var lineTotal = SumCreateOrderLines(command.Items);
            dto.AmountOutstanding.Should().Be(lineTotal - dto.AmountPaid);

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
            dto.Status.Should().Be(OrderStatus.Pending);
            dto.PaymentType.Should().Be(command.PaymentType);
            dto.AmountPaid.Should().Be(0);
            dto.Payments.Should().BeEmpty();
            dto.AmountOutstanding.Should().Be(SumCreateOrderLines(command.Items));
            dto.PaymentStatus.Should().Be(OrderPaymentStatus.Unpaid);
            dto.AssertOrderCustomer(expectedCustomer);
            dto.Items.Should().HaveCount(command.Items.Count);

            for (int i = 0; i < command.Items.Count; i++)
            {
                var expected = resolveExpectedProduct(command.Items[i].ProductId);
                dto.Items[i].AssertOrderItemDto(command.Items[i], expected);
            }
        }

        public static void AssertOrderPricing(this OrderDto dto, Order order)
        {
            dto.TotalOrderAmount.Should().Be(order.TotalOrderAmount);
            dto.NetOfTotalOrderAmount.Should().Be(order.NetOfTotalOrderAmount);
            dto.ReturnsTotal.Should().Be(order.TotalOrderAmount - order.NetOfTotalOrderAmount);
            dto.AmountOutstanding.Should().Be(order.AmountOutstanding);
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
                dto.Product.Price.Should().Be(expectedProduct.Price);
            }
        }
    }
}
