using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Invoria.BuildingBlocks.Application.Requests;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Infrastructure.Common;
using Invoria.Catalog.Contracts.Dtos;
using Invoria.Catalog.Domain.Products;
using Invoria.Endpoints.Tests.Utilities;
using Microsoft.AspNetCore.WebUtilities;
using NUnit.Framework;

namespace Invoria.Catalog.Endpoints.Tests.Products;

[TestFixture]
public class ListProductsEndpointTests : ProductEndpointTestFixture
{
    [Test]
    public async Task Should_return_paged_list_of_products()
    {
        var uniqueKey = Guid.NewGuid().ToString("N");
        var product = new Product($"Test Product {uniqueKey}", $"TEST-CODE-{uniqueKey}", 10);

        await ProductRepository.Add(product);
        await BatchRepository.Add(new Invoria.Inventory.Domain.Batches.Batch(product.Id, 12, 10m));

        var queryParams = new Dictionary<string, string?>
        {
            ["Skip"] = "0",
            ["Length"] = "10",
            ["Name"] = uniqueKey
        };
        var uri = QueryHelpers.AddQueryString("/products", queryParams);

        var response = await Client.GetAsync(uri);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PagingDto<ProductDto>>>();

        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeTrue();
        envelope.Result.Should().NotBeNull();
        envelope.Result!.Data.Should().NotBeNullOrEmpty();
        var dto = envelope.Result.Data.First(x => x.Id == product.Id);
        dto.Stock.Should().NotBeNull();
        dto.Stock!.ActualQuantity.Should().Be(12);
        dto.Stock.ReservedQuantity.Should().Be(0);
    }

    [Test]
    public async Task Should_filter_products_by_name_when_name_query_param_is_provided()
    {
        var matchingProduct = new Product("Gaming Mouse", "MOUSE-01", 50);
        var nonMatchingProduct = new Product("Office Keyboard", "KEYBOARD-01", 30);

        await ProductRepository.Add(matchingProduct);
        await ProductRepository.Add(nonMatchingProduct);
        await BatchRepository.Add(new Invoria.Inventory.Domain.Batches.Batch(matchingProduct.Id, 5, 10m));

        var queryParams = new Dictionary<string, string?>
        {
            ["Skip"] = "0",
            ["Length"] = "10",
            ["Name"] = "gaming"
        };

        var uri = QueryHelpers.AddQueryString("/products", queryParams);

        var response = await Client.GetAsync(uri);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PagingDto<ProductDto>>>();

        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeTrue();
        envelope.Result.Should().NotBeNull();
        envelope.Result!.Data.Should().Contain(x => x.Name == "Gaming Mouse");
        var gamingMouse = envelope.Result.Data.First(x => x.Id == matchingProduct.Id);
        gamingMouse.Stock.Should().NotBeNull();
        gamingMouse.Stock!.ActualQuantity.Should().Be(5);
        gamingMouse.Stock.ReservedQuantity.Should().Be(0);
    }

    [Test]
    public async Task Should_return_validation_errors_envelope_when_request_is_invalid()
    {
        var request = new PagingParams { Skip = 0, Length = 0 };

        string uri = "/products?" + QueryStringHelper.ToQueryString(request);

        var response = await Client.GetAsync(uri);

        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope>();

        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeFalse();
        envelope.Error.Should().NotBeNull();
        envelope.Error!.Status.Should().Be((int)HttpStatusCode.BadRequest);
        envelope.Error.Errors.Should().NotBeEmpty();
        envelope.Error.Errors.Keys.Should().Contain(x => x.Contains("Length") || x == "GeneralErrors");
    }
}

