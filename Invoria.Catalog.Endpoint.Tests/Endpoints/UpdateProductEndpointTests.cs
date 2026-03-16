using FluentAssertions;
using Invoria.Modules.Catalog.Domain;
using Invoria.Modules.Catalog.Domain.Products;
using Invoria.Modules.Catalog.Endpoints.Products.Requests;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;

namespace Invoria.Catalog.Endpoint.Tests.Endpoints
{
    [TestFixture]
    public class UpdateProductEndpointTests : CatalogTestFixture
    {
        public ICatalogRepository<Product> Product { get; set; }

        public UpdateProductEndpointTests()
        {
            Product = Scope.ServiceProvider.GetRequiredService<ICatalogRepository<Product>>();
        }

        [Test]
        public async Task Should_update_prdouct()
        {

            var fakeProduct = await CreateProductAsync();

            var request = new UpdateProductRequest()
            {
                Id = fakeProduct.Id,
                Name = Guid.NewGuid().ToString(),
                Code = Guid.NewGuid().ToString(),
                Price = 65
            };

            var response = await Client.PutAsJsonAsync("/products/"+ fakeProduct.Id, request);

            response.IsSuccessStatusCode.Should().BeTrue();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Test]
        public async Task Should_return_failure_status_code_404_when_product_is_not_exist()
        {
            var request = new UpdateProductRequest()
            {
                Id = Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString(),
                Code = Guid.NewGuid().ToString(),
                Price = 65
            };

            var response = await Client.PutAsJsonAsync("/products/" + request.Id, request);

            response.IsSuccessStatusCode.Should().BeFalse();

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }

        public async Task<Product> CreateProductAsync()
        {
            var product = new Product(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), 440);

            return await  Product.Add(product);
        }
    }
}
