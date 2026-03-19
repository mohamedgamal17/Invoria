using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.CustomerManagement.Application.Customers.Queries.GetCustomerById;
using Invoria.CustomerManagement.Application.Tests.Assertions;
using Invoria.CustomerManagement.Domain.Customers;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.CustomerManagement.Application.Tests.Customers
{
    [TestFixture]
    public class GetCustomerByIdQueryHandlerTests : CustomerTestFixture
    {
        protected ICustomerRepository<Customer> CustomerRepository { get; }
        protected IMediator Mediator { get; }

        public GetCustomerByIdQueryHandlerTests()
        {
            CustomerRepository = ServiceProvider.GetRequiredService<ICustomerRepository<Customer>>();
            Mediator = ServiceProvider.GetRequiredService<IMediator>();
        }

        [Test]
        public async Task Should_return_customer_when_found()
        {
            // Arrange
            var initialCustomer = new Customer("John Doe");
            await CustomerRepository.Add(initialCustomer);

            var query = new GetCustomerByIdQuery
            {
                Id = initialCustomer.Id
            };

            // Act
            var result = await Mediator.Send(query);

            // Assert
            result.ShouldBeSuccess();
            result.Value!.AssertCustomerDto(initialCustomer);
        }

        [Test]
        public async Task Should_return_failure_when_customer_not_found()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid().ToString();
            var query = new GetCustomerByIdQuery
            {
                Id = nonExistentId
            };

            // Act
            var result = await Mediator.Send(query);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Exception.Should().NotBeNull();
            result.Exception.Should().BeOfType<NotFoundException>();
        }
    }
}

