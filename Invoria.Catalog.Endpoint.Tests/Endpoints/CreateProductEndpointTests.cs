

using FluentAssertions;
using Invoria.Modules.Catalog.Endpoints.Products.Requests;
using System.Net.Http.Json;

namespace Invoria.Catalog.Endpoint.Tests.Endpoints
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
                Code = Guid.NewGuid().ToString(),
                Price = 65
            };

            var response = await Client.PostAsJsonAsync("/products", request);

            response.IsSuccessStatusCode.Should().BeTrue();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }
    }
}
