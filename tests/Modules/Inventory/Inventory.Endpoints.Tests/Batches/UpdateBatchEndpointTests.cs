using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Invoria.Inventory.Endpoints.Batches.Requests;

namespace Invoria.Inventory.Endpoints.Tests.Batches;

[TestFixture]
public class UpdateBatchEndpointTests : InventoryTestFixture
{
    [Test]
    public async Task Should_update_batch()
    {
        var createRequest = new CreateBatchRequest
        {
            ProductId = Guid.NewGuid().ToString(),
            Quantity = 4,
            PurchasePrice = 12.75m
        };

        var createResponse = await Client.PostAsJsonAsync("/batches", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<BatchEnvelope>();

        var updateRequest = new UpdateBatchRequest
        {
            Id = created!.Result!.Id,
            Quantity = 8,
            PurchasePrice = 20m
        };

        var updateResponse = await Client.PutAsJsonAsync($"/batches/{updateRequest.Id}", updateRequest);

        updateResponse.IsSuccessStatusCode.Should().BeTrue();
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task Should_allow_zero_quantity()
    {
        var createRequest = new CreateBatchRequest
        {
            ProductId = Guid.NewGuid().ToString(),
            Quantity = 4,
            PurchasePrice = 12.75m
        };

        var createResponse = await Client.PostAsJsonAsync("/batches", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<BatchEnvelope>();

        var updateRequest = new UpdateBatchRequest
        {
            Id = created!.Result!.Id,
            Quantity = 0,
            PurchasePrice = 20m
        };

        var updateResponse = await Client.PutAsJsonAsync($"/batches/{updateRequest.Id}", updateRequest);

        updateResponse.IsSuccessStatusCode.Should().BeTrue();
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task Should_return_not_found_when_batch_not_exist()
    {
        var updateRequest = new UpdateBatchRequest
        {
            Id = Guid.NewGuid().ToString(),
            Quantity = 0,
            PurchasePrice = 20m
        };

        var updateResponse = await Client.PutAsJsonAsync($"/batches/{updateRequest.Id}", updateRequest);

        updateResponse.IsSuccessStatusCode.Should().BeFalse();
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task Should_fail_when_quantity_is_negative()
    {
        var updateRequest = new UpdateBatchRequest
        {
            Id = Guid.NewGuid().ToString(),
            Quantity = -1,
            PurchasePrice = 20m
        };

        var updateResponse = await Client.PutAsJsonAsync($"/batches/{updateRequest.Id}", updateRequest);

        updateResponse.IsSuccessStatusCode.Should().BeFalse();
        updateResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private class BatchEnvelope
    {
        public BatchResult? Result { get; set; }
    }

    private class BatchResult
    {
        public string Id { get; set; } = string.Empty;
    }
}
