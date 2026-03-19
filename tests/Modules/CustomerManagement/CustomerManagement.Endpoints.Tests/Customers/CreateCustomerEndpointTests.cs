using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Invoria.CustomerManagement.Endpoints.Customers.Requests;
using NUnit.Framework;

namespace Invoria.CustomerManagement.Endpoints.Tests.Customers
{
    [TestFixture]
    public class CreateCustomerEndpointTests : CustomerTestFixture
    {
        [Test]
        public async Task Should_create_customer()
        {
            var request = new CreateCustomerRequest
            {
                Name = Guid.NewGuid().ToString()
            };

            var response = await Client.PostAsJsonAsync("/customers", request);

            response.IsSuccessStatusCode.Should().BeTrue();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}

