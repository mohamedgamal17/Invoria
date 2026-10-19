using FluentAssertions;
using Invoria.Ordering.Contracts.Orders.Dtos;
using Invoria.Ordering.Domain.Orders;

namespace Invoria.Ordering.Application.Tests.Assertions
{
    public static class PublicOrderAssertionExtensions
    {
        public static void AssertPublicOrderDto(this PublicOrderDto dto, Order order)
        {
            dto.OrderNumber.Should().Be(order.OrderNumber);
            dto.CustomerId.Should().Be(order.CustomerId);
            dto.Status.Should().Be(order.Status);
            dto.PaymentType.Should().Be(order.PaymentType);
            dto.SubtotalAmount.Should().Be(order.TotalOrderAmount);
            dto.ReturnsAmount.Should().Be(order.TotalOrderAmount - order.NetOfTotalOrderAmount);
            dto.NetOrderAmount.Should().Be(order.NetOfTotalOrderAmount);
            dto.AmountOutstanding.Should().Be(order.AmountOutstanding);
            dto.AllocationId.Should().Be(order.AllocationId);
            dto.ReturnId.Should().Be(order.ReturnId);
            dto.InvoiceId.Should().Be(order.InvoiceId);
            dto.OrderAllocated.Should().Be(order.OrderAllocated);
        }
    }
}
