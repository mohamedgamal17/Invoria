using FluentAssertions;
using Invoria.CustomerManagement.Contracts.Dtos;
using Invoria.CustomerManagement.Domain.Customers;

namespace Invoria.CustomerManagement.Application.Tests.Assertions
{
    public static class CustomerAssertionExtensions
    {
        public static void AssertCustomerDto(this CustomerDto dto, Customer customer)
        {
            dto.Id.Should().Be(customer.Id);
            dto.Name.Should().Be(customer.Name);
        }
    }
}

