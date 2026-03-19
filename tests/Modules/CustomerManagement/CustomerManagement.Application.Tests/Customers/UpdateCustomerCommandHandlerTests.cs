using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.CustomerManagement.Application.Customers.Commands.UpdateCustomer;
using Invoria.CustomerManagement.Application.Tests.Assertions;
using Invoria.CustomerManagement.Domain.Customers;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.CustomerManagement.Application.Tests.Customers
{
    [TestFixture]
    public class UpdateCustomerCommandHandlerTests : CustomerTestFixture
    {
        protected ICustomerRepository<Customer> CustomerRepository { get; }
        protected IMediator Mediator { get; }

        public UpdateCustomerCommandHandlerTests()
        {
            CustomerRepository = ServiceProvider.GetRequiredService<ICustomerRepository<Customer>>();
            Mediator = ServiceProvider.GetRequiredService<IMediator>();
        }

        [Test]
        public async Task Should_update_customer()
        {
            // Arrange
            var initialCustomer = new Customer("Old Name");
            await CustomerRepository.Add(initialCustomer);

            var command = new UpdateCustomerCommand(initialCustomer.Id, "New Name");

            // Act
            var result = await Mediator.Send(command);

            // Assert
            result.ShouldBeSuccess();

            var updatedCustomer = await CustomerRepository.SingleOrDefault(x => x.Id == initialCustomer.Id);
            updatedCustomer.Should().NotBeNull();

            updatedCustomer!.Name.Should().Be("New Name");
            result.Value!.AssertCustomerDto(updatedCustomer);
        }

        [Test]
        public async Task Should_return_failure_when_customer_not_found()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid().ToString();
            var command = new UpdateCustomerCommand(nonExistentId, "New Name");

            // Act
            var result = await Mediator.Send(command);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Exception.Should().NotBeNull();
            result.Exception.Should().BeOfType<NotFoundException>();
        }
    }
}

