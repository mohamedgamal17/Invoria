using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Invoria.BuildingBlocks.Infrastructure.Common;
using Invoria.CustomerManagement.Contracts.Dtos;
using Invoria.CustomerManagement.Domain.Customers;
using NUnit.Framework;

namespace Invoria.CustomerManagement.Endpoints.Tests.Customers
{
    [TestFixture]
    public class GetCustomerByIdEndpointTests : CustomerEndpointTestFixture
    {
        [Test]
        public async Task Should_return_customer_when_found()
        {
            var customer = new Customer(Guid.NewGuid().ToString());
            await CustomerRepository.Add(customer);

            var response = await Client.GetAsync("/customers/" + customer.Id);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var envelope = await response.Content.ReadFromJsonAsync<Envelope<CustomerDto>>();
            envelope.Should().NotBeNull();
            envelope!.IsSuccess.Should().BeTrue();
            envelope.Result.Should().NotBeNull();
            envelope.Result!.Id.Should().Be(customer.Id);
            envelope.Result.Name.Should().Be(customer.Name);
        }

        [Test]
        public async Task Should_return_404_when_customer_not_found()
        {
            var nonExistentId = Guid.NewGuid().ToString();

            var response = await Client.GetAsync("/customers/" + nonExistentId);

            response.IsSuccessStatusCode.Should().BeFalse();
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}

