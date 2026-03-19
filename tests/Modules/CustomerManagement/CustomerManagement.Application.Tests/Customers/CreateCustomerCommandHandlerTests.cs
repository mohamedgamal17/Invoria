using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.CustomerManagement.Application.Customers.Commands.CreateCustomer;
using Invoria.CustomerManagement.Application.Tests.Assertions;
using Invoria.CustomerManagement.Domain.Customers;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.CustomerManagement.Application.Tests.Customers
{
    [TestFixture]
    public class CreateCustomerCommandHandlerTests : CustomerTestFixture
    {
        protected ICustomerRepository<Customer> CustomerRepository { get; }
        protected IMediator Mediator { get; }

        public CreateCustomerCommandHandlerTests()
        {
            CustomerRepository = ServiceProvider.GetRequiredService<ICustomerRepository<Customer>>();
            Mediator = ServiceProvider.GetRequiredService<IMediator>();
        }

        [Test]
        public async Task Should_create_customer()
        {
            var command = new CreateCustomerCommand("John Doe");

            var result = await Mediator.Send(command);

            result.ShouldBeSuccess();

            var customerId = result.Value!.Id;
            var customer = await CustomerRepository.SingleOrDefault(x => x.Id == customerId);
            customer.Should().NotBeNull();

            result.Value!.AssertCustomerDto(customer!);
        }
    }
}

