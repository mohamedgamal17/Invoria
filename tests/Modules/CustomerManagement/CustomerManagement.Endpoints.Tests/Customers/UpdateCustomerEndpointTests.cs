using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Invoria.CustomerManagement.Domain.Customers;
using Invoria.CustomerManagement.Endpoints.Customers.Requests;
using NUnit.Framework;

namespace Invoria.CustomerManagement.Endpoints.Tests.Customers
{
    [TestFixture]
    public class UpdateCustomerEndpointTests : CustomerEndpointTestFixture
    {
        [Test]
        public async Task Should_update_customer()
        {
            var fakeCustomer = await CreateCustomerAsync();

            var request = new UpdateCustomerRequest
            {
                Id = fakeCustomer.Id,
                Name = Guid.NewGuid().ToString()
            };

            var response = await Client.PutAsJsonAsync("/customers/" + fakeCustomer.Id, request);

            response.IsSuccessStatusCode.Should().BeTrue();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task Should_return_failure_status_code_404_when_customer_is_not_exist()
        {
            var request = new UpdateCustomerRequest
            {
                Id = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString()
            };

            var response = await Client.PutAsJsonAsync("/customers/" + request.Id, request);

            response.IsSuccessStatusCode.Should().BeFalse();
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        private async Task<Customer> CreateCustomerAsync()
        {
            var customer = new Customer(Guid.NewGuid().ToString());

            return await CustomerRepository.Add(customer);
        }
    }
}

