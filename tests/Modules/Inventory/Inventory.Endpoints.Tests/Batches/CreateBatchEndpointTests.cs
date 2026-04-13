using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Invoria.BuildingBlocks.Infrastructure.Common;
using Invoria.Inventory.Endpoints.Batches.Requests;

namespace Invoria.Inventory.Endpoints.Tests.Batches;

[TestFixture]
public class CreateBatchEndpointTests : InventoryTestFixture
{
    [Test]
    public async Task Should_create_batch()
    {
        var request = new CreateBatchRequest
        {
            ProductId = Guid.NewGuid().ToString(),
            Quantity = 4,
            PurchasePrice = 12.75m
        };

        var response = await Client.PostAsJsonAsync("/batches", request);

        response.IsSuccessStatusCode.Should().BeTrue();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task Should_fail_when_quantity_is_negative()
    {
        var request = new CreateBatchRequest
        {
            ProductId = Guid.NewGuid().ToString(),
            Quantity = -1,
            PurchasePrice = 12.75m
        };

        var response = await Client.PostAsJsonAsync("/batches", request);

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
