using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Invoria.BuildingBlocks.Infrastructure.Common;
using Invoria.Ordering.Contracts.Dtos;
using Invoria.Ordering.Endpoints.Orders.Requests;

namespace Invoria.Ordering.Endpoints.Tests.Orders;

[TestFixture]
public class UpdateOrderItemsEndpointTests : OrderingTestFixture
{
    [Test]
    public async Task Should_update_order_items()
    {
        var productId1 = Guid.NewGuid().ToString();
        var productId2 = Guid.NewGuid().ToString();
        var customerId = Guid.NewGuid().ToString();

        var createRequest = new CreateOrderRequest
        {
            CustomerId = customerId,
            Items =
            [
                new CreateOrderLineItemRequest { ProductId = productId1, Quantity = 2, Price = 10m }
            ]
        };

        var createResponse = await Client.PostAsJsonAsync("/orders", createRequest);
        createResponse.IsSuccessStatusCode.Should().BeTrue();
        var createEnvelope = await createResponse.Content.ReadFromJsonAsync<Envelope<OrderDto>>();
        createEnvelope.Should().NotBeNull();
        var created = createEnvelope!.Result!;
        created.Id.Should().NotBeNullOrEmpty();

        var updateRequest = new UpdateOrderItemsRequest
        {
            Id = created.Id,
            Items =
            [
                new CreateOrderLineItemRequest { ProductId = productId2, Quantity = 3, Price = 15.5m },
                new CreateOrderLineItemRequest { ProductId = productId1, Quantity = 1, Price = 10m }
            ]
        };

        var updateResponse = await Client.PutAsJsonAsync($"/orders/{created.Id}", updateRequest);

        updateResponse.IsSuccessStatusCode.Should().BeTrue();
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updateEnvelope = await updateResponse.Content.ReadFromJsonAsync<Envelope<OrderDto>>();
        updateEnvelope.Should().NotBeNull();
        var updated = updateEnvelope!.Result!;
        updated.Items.Should().HaveCount(2);
        updated.Items.Should().Contain(i => i.ProductId == productId2 && i.Quantity == 3 && i.Price == 15.5m);
    }

    [Test]
    public async Task Should_fail_when_items_is_empty()
    {
        var createRequest = new CreateOrderRequest
        {
            CustomerId = Guid.NewGuid().ToString(),
            Items =
            [
                new CreateOrderLineItemRequest { ProductId = Guid.NewGuid().ToString(), Quantity = 1, Price = 1m }
            ]
        };

        var createResponse = await Client.PostAsJsonAsync("/orders", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var createEnvelope = await createResponse.Content.ReadFromJsonAsync<Envelope<OrderDto>>();
        var created = createEnvelope!.Result!;

        var updateRequest = new UpdateOrderItemsRequest
        {
            Id = created.Id,
            Items = []
        };

        var updateResponse = await Client.PutAsJsonAsync($"/orders/{created.Id}", updateRequest);

        updateResponse.IsSuccessStatusCode.Should().BeFalse();
        updateResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
