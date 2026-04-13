using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Invoria.BuildingBlocks.Infrastructure.Common;
using Invoria.Ordering.Contracts.Dtos;
using Invoria.Ordering.Endpoints.Orders.Requests;

namespace Invoria.Ordering.Endpoints.Tests.Orders;

[TestFixture]
public class CreateOrderEndpointTests : OrderingTestFixture
{
    [Test]
    public async Task Should_create_order()
    {
        var productId1 = Guid.NewGuid().ToString();
        var productId2 = Guid.NewGuid().ToString();
        var customerId = Guid.NewGuid().ToString();

        var request = new CreateOrderRequest
        {
            CustomerId = customerId,
            Items =
            [
                new CreateOrderLineItemRequest { ProductId = productId1, Quantity = 2, Price = 10m },
                new CreateOrderLineItemRequest { ProductId = productId2, Quantity = 1, Price = 25m }
            ]
        };

        var response = await Client.PostAsJsonAsync("/orders", request);

        response.IsSuccessStatusCode.Should().BeTrue();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task Should_fail_when_items_is_empty()
    {
        var request = new CreateOrderRequest
        {
            CustomerId = Guid.NewGuid().ToString(),
            Items = []
        };

        var response = await Client.PostAsJsonAsync("/orders", request);

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
