using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Invoria.BuildingBlocks.Infrastructure.Common;
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

        [Test]
        public async Task Should_return_validation_errors_envelope_when_request_is_invalid()
        {
            var request = new CreateCustomerRequest
            {
                Name = ""
            };

            var response = await Client.PostAsJsonAsync("/customers", request);

            response.IsSuccessStatusCode.Should().BeFalse();
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var envelope = await response.Content.ReadFromJsonAsync<Envelope>();

            envelope.Should().NotBeNull();
            envelope!.IsSuccess.Should().BeFalse();
            envelope.Error.Should().NotBeNull();
            envelope.Error!.Status.Should().Be((int)HttpStatusCode.BadRequest);
            envelope.Error.Errors.Should().NotBeEmpty();
        }
    }
}

