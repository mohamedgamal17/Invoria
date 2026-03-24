using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Invoria.BuildingBlocks.Infrastructure.Common;
using Invoria.Ordering.Contracts.Dtos;
using Invoria.Ordering.Endpoints.Orders.Requests;

namespace Invoria.Ordering.Endpoints.Tests.Orders;

[TestFixture]
public class AcceptOrderEndpointTests : OrderingTestFixture
{
    [Test]
    public async Task Should_accept_order()
    {
        var productId = Guid.NewGuid().ToString();
        var customerId = Guid.NewGuid().ToString();

        var createRequest = new CreateOrderRequest
        {
            CustomerId = customerId,
            Items =
            [
                new CreateOrderLineItemRequest { ProductId = productId, Quantity = 2, Price = 10m }
            ]
        };

        var createResponse = await Client.PostAsJsonAsync("/orders", createRequest);
        createResponse.IsSuccessStatusCode.Should().BeTrue();
        var createEnvelope = await createResponse.Content.ReadFromJsonAsync<Envelope<OrderDto>>();
        createEnvelope.Should().NotBeNull();
        var created = createEnvelope!.Result!;
        created.Id.Should().NotBeNullOrEmpty();

        var acceptResponse = await Client.PostAsync(
            $"/orders/{created.Id}/accept",
            new StringContent("{}", Encoding.UTF8, "application/json"));

        acceptResponse.IsSuccessStatusCode.Should().BeTrue();
        acceptResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var acceptEnvelope = await acceptResponse.Content.ReadFromJsonAsync<Envelope<OrderDto>>();
        acceptEnvelope.Should().NotBeNull();
        acceptEnvelope!.Result!.Id.Should().Be(created.Id);
    }

    [Test]
    public async Task Should_fail_when_order_cannot_be_accepted()
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

        var emptyJson = new StringContent("{}", Encoding.UTF8, "application/json");
        var firstAccept = await Client.PostAsync($"/orders/{created.Id}/accept", emptyJson);
        firstAccept.EnsureSuccessStatusCode();

        var secondAccept = await Client.PostAsync(
            $"/orders/{created.Id}/accept",
            new StringContent("{}", Encoding.UTF8, "application/json"));

        secondAccept.IsSuccessStatusCode.Should().BeFalse();
        secondAccept.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
