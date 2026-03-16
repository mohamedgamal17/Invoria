using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Invoria.BuildingBlocks.Infrastructure.Common;
using Invoria.Catalog.Contracts.Dtos;
using Invoria.Catalog.Domain.Products;
using NUnit.Framework;

namespace Invoria.Catalog.Endpoints.Tests.Products;

[TestFixture]
public class GetProductByIdEndpointTests : ProductEndpointTestFixture
{
    [Test]
    public async Task Should_return_product_when_found()
    {
        var product = new Product("Test Product", "TEST-CODE", 10);
        await ProductRepository.Add(product);

        var response = await Client.GetAsync("/products/" + product.Id);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<ProductDto>>();

        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeTrue();
        envelope.Result.Should().NotBeNull();
        envelope.Result!.Id.Should().Be(product.Id);
        envelope.Result.Name.Should().Be(product.Name);
        envelope.Result.Code.Should().Be(product.Code);
        envelope.Result.Price.Should().Be(product.Price);
    }

    [Test]
    public async Task Should_return_404_when_product_not_found()
    {
        var nonExistentId = Guid.NewGuid().ToString();

        var response = await Client.GetAsync("/products/" + nonExistentId);

        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
