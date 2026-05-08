using FluentAssertions;
using Invoria.BuildingBlocks.Infrastructure.Common;
using Invoria.Catalog.Endpoints.Products.Requests;
using System.Net.Http.Json;
using System.Net;

namespace Invoria.Catalog.Endpoints.Tests.Products
{
    [TestFixture]
    public class CreateProductEndpointTests : CatalogTestFixture
    {
        [Test]
        public async Task Should_create_prdouct()
        {
            var request = new CreateProductRequest()
            {
                Name = Guid.NewGuid().ToString(),
                Price = 65
            };

            var response = await Client.PostAsJsonAsync("/products", request);

            response.IsSuccessStatusCode.Should().BeTrue();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Test]
        public async Task Should_return_validation_errors_envelope_when_request_is_invalid()
        {
            var request = new CreateProductRequest
            {
                Name = "a",
                Price = 0
            };

            var response = await Client.PostAsJsonAsync("/products", request);

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
