using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Invoria.BuildingBlocks.Infrastructure.Common;
using Invoria.Inventory.Contracts.Dtos;
using Invoria.Inventory.Endpoints.Batches.Requests;

namespace Invoria.Inventory.Endpoints.Tests.Batches;

[TestFixture]
public class GetBatchByIdEndpointTests : InventoryTestFixture
{
    [Test]
    public async Task Should_return_batch_when_found()
    {
        var createRequest = new CreateBatchRequest
        {
            ProductId = Guid.NewGuid().ToString(),
            Quantity = 6,
            PurchasePrice = 18.5m
        };

        var createResponse = await Client.PostAsJsonAsync("/batches", createRequest);
        createResponse.EnsureSuccessStatusCode();

        var created = await createResponse.Content.ReadFromJsonAsync<Envelope<BatchDto>>();

        var response = await Client.GetAsync("/batches/" + created!.Result!.Id);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<BatchDto>>();
        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeTrue();
        envelope.Result.Should().NotBeNull();
        envelope.Result!.Id.Should().Be(created.Result.Id);
        envelope.Result.ProductId.Should().Be(createRequest.ProductId);
    }

    [Test]
    public async Task Should_return_404_when_batch_not_found()
    {
        var response = await Client.GetAsync("/batches/" + Guid.NewGuid());

        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
