using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Infrastructure.Common;
using Invoria.Endpoints.Tests.Utilities;
using Invoria.Inventory.Contracts.Batches.Dtos;
using Invoria.Inventory.Domain.Batches;
using Invoria.Inventory.Endpoints.Batches.Requests;

namespace Invoria.Inventory.Endpoints.Tests.Batches;

[TestFixture]
public class ListBatchesEndpointTests : InventoryTestFixture
{
    [Test]
    public async Task Should_return_paged_list_including_created_batch()
    {
        var productId = Guid.NewGuid().ToString();

        var createRequest = new CreateBatchRequest
        {
            ProductId = productId,
            Quantity = 4,
            PurchasePrice = 12.75m
        };

        var createResponse = await Client.PostAsJsonAsync("/batches", createRequest);
        createResponse.IsSuccessStatusCode.Should().BeTrue();

        var query = new { Skip = 0, Length = 100 };
        var uri = "/batches?" + QueryStringHelper.ToQueryString(query);

        var response = await Client.GetAsync(uri);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PagingDto<BatchDto>>>();
        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeTrue();
        envelope.Result.Should().NotBeNull();
        envelope.Result!.Data.Should().Contain(x => x.ProductId == productId);
    }

    [Test]
    public async Task Should_filter_by_product_id_and_state()
    {
        var productId = Guid.NewGuid().ToString();

        var activeRequest = new CreateBatchRequest
        {
            ProductId = productId,
            Quantity = 5,
            PurchasePrice = 10m
        };
        var activeResponse = await Client.PostAsJsonAsync("/batches", activeRequest);
        activeResponse.EnsureSuccessStatusCode();

        var depletedRequest = new CreateBatchRequest
        {
            ProductId = productId,
            Quantity = 3,
            PurchasePrice = 9m
        };
        var depletedCreateResponse = await Client.PostAsJsonAsync("/batches", depletedRequest);
        depletedCreateResponse.EnsureSuccessStatusCode();

        var depletedCreated = await depletedCreateResponse.Content.ReadFromJsonAsync<Envelope<BatchDto>>();
        var updateRequest = new UpdateBatchRequest
        {
            Id = depletedCreated!.Result!.Id,
            Quantity = 0,
            PurchasePrice = depletedCreated.Result.PurchasePrice
        };
        var updateResponse = await Client.PutAsJsonAsync($"/batches/{updateRequest.Id}", updateRequest);
        updateResponse.EnsureSuccessStatusCode();

        var query = new ListBatchesRequest
        {
            Skip = 0,
            Length = 100,
            ProductId = productId,
            State = BatchState.Depleted
        };
        var uri = "/batches?" + QueryStringHelper.ToQueryString(query);

        var response = await Client.GetAsync(uri);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PagingDto<BatchDto>>>();
        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeTrue();
        envelope.Result.Should().NotBeNull();
        envelope.Result!.Data.Should().HaveCount(1);
        envelope.Result.Data.Single().ProductId.Should().Be(productId);
    }
}
