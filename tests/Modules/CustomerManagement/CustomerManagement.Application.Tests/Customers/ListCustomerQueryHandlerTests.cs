using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.CustomerManagement.Application.Customers.Queries.ListCustomers;
using Invoria.CustomerManagement.Domain.Customers;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.CustomerManagement.Application.Tests.Customers
{
    [TestFixture]
    public class ListCustomerQueryHandlerTests : CustomerTestFixture
    {
        protected ICustomerRepository<Customer> CustomerRepository { get; }
        protected IMediator Mediator { get; }

        public ListCustomerQueryHandlerTests()
        {
            CustomerRepository = ServiceProvider.GetRequiredService<ICustomerRepository<Customer>>();
            Mediator = ServiceProvider.GetRequiredService<IMediator>();
        }

        [Test]
        public async Task Should_return_only_customers_matching_name_filter()
        {
            // Arrange
            var matchingCustomerOne = new Customer($"Acme {GetUniqueSuffix()}");
            var matchingCustomerTwo = new Customer($"acme {GetUniqueSuffix()}");
            var nonMatchingCustomer = new Customer($"Other {GetUniqueSuffix()}");

            await CustomerRepository.Add(matchingCustomerOne);
            await CustomerRepository.Add(matchingCustomerTwo);
            await CustomerRepository.Add(nonMatchingCustomer);

            var query = new ListCustomerQuery
            {
                Skip = 0,
                Length = 50,
                Name = " acme "
            };

            // Act
            var result = await Mediator.Send(query);

            // Assert
            result.ShouldBeSuccess();
            result.Value.Should().NotBeNull();
            result.Value.Data.Should().OnlyContain(x => x.Name.ToLower().Contains("acme"));
            result.Value.Data.Should().Contain(x => x.Id == matchingCustomerOne.Id);
            result.Value.Data.Should().Contain(x => x.Id == matchingCustomerTwo.Id);
        }

        [Test]
        public async Task Should_ignore_whitespace_name_filter()
        {
            // Arrange
            var customerOne = new Customer($"Customer {GetUniqueSuffix()}");
            var customerTwo = new Customer($"Client {GetUniqueSuffix()}");

            await CustomerRepository.Add(customerOne);
            await CustomerRepository.Add(customerTwo);

            var query = new ListCustomerQuery
            {
                Skip = 0,
                Length = 50,
                Name = "   "
            };

            // Act
            var result = await Mediator.Send(query);

            // Assert
            result.ShouldBeSuccess();
            result.Value.Should().NotBeNull();
            result.Value!.Data.Should().Contain(x => x.Id == customerOne.Id);
            result.Value.Data.Should().Contain(x => x.Id == customerTwo.Id);
        }

        [Test]
        public async Task Should_return_customers_ordered_by_id_descending()
        {
            var customerOne = new Customer($"Order A {GetUniqueSuffix()}");
            var customerTwo = new Customer($"Order B {GetUniqueSuffix()}");
            var customerThree = new Customer($"Order C {GetUniqueSuffix()}");

            await CustomerRepository.Add(customerOne);
            await CustomerRepository.Add(customerTwo);
            await CustomerRepository.Add(customerThree);

            var query = new ListCustomerQuery
            {
                Skip = 0,
                Length = 3
            };

            var result = await Mediator.Send(query);

            result.ShouldBeSuccess();
            result.Value.Should().NotBeNull();

            var expectedIds = new[] { customerOne.Id, customerTwo.Id, customerThree.Id }
                .OrderByDescending(x => x)
                .ToList();
            result.Value!.Data.Select(x => x.Id).Should().Equal(expectedIds);
        }

        private static string GetUniqueSuffix()
        {
            return Guid.NewGuid().ToString("N")[..8];
        }
    }
}
