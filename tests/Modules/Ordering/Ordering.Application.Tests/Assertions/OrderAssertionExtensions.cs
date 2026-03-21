using FluentAssertions;
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
                dto.Items[i].AssertOrderItemDto(command.Items[i]);
            }
        }

        public static void AssertOrderItemDto(this OrderItemDto dto, CreateOrderItemCommand commandItem)
        {
            dto.ProductId.Should().Be(commandItem.ProductId);
            dto.Quantity.Should().Be(commandItem.Quantity);
            dto.Price.Should().Be(commandItem.Price);
        }
    }
}
