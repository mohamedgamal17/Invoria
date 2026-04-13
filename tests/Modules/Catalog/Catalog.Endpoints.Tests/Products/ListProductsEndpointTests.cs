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
        var product = new Product("Test Product", "TEST-CODE", 10);

        await ProductRepository.Add(product);

        var request = new PagingParams { Skip = 0, Length = 10 };

        string uri = "/products?" + QueryStringHelper.ToQueryString(request);

        var response = await Client.GetAsync(uri);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PagingDto<ProductDto>>>();

        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeTrue();
        envelope.Result.Should().NotBeNull();
        envelope.Result!.Data.Should().NotBeNullOrEmpty();
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

