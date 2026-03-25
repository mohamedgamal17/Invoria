using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Invoria.BuildingBlocks.Infrastructure.Common;
using Invoria.Ordering.Contracts.Dtos;
using Invoria.Ordering.Endpoints.Orders.Requests;

namespace Invoria.Ordering.Endpoints.Tests.Orders;

[TestFixture]
public class ReopenOrderEndpointTests : OrderingTestFixture
{
    [Test]
    public async Task Should_reopen_order()
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

        var emptyJson = new StringContent("{}", Encoding.UTF8, "application/json");
        var acceptResponse = await Client.PostAsync($"/orders/{created.Id}/accept", emptyJson);
        acceptResponse.EnsureSuccessStatusCode();

        var reopenResponse = await Client.PostAsync(
            $"/orders/{created.Id}/reopen",
            new StringContent("{}", Encoding.UTF8, "application/json"));

        reopenResponse.IsSuccessStatusCode.Should().BeTrue();
        reopenResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var reopenEnvelope = await reopenResponse.Content.ReadFromJsonAsync<Envelope<OrderDto>>();
        reopenEnvelope.Should().NotBeNull();
        reopenEnvelope!.Result!.Id.Should().Be(created.Id);
    }

    [Test]
    public async Task Should_fail_when_order_cannot_be_reopened()
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
        var firstReopen = await Client.PostAsync($"/orders/{created.Id}/reopen", emptyJson);
        firstReopen.IsSuccessStatusCode.Should().BeFalse();
        firstReopen.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        var acceptResponse = await Client.PostAsync($"/orders/{created.Id}/accept", emptyJson);
        acceptResponse.EnsureSuccessStatusCode();

        var firstReopenAfterAccept = await Client.PostAsync($"/orders/{created.Id}/reopen", emptyJson);
        firstReopenAfterAccept.EnsureSuccessStatusCode();

        var secondReopen = await Client.PostAsync(
            $"/orders/{created.Id}/reopen",
            new StringContent("{}", Encoding.UTF8, "application/json"));

        secondReopen.IsSuccessStatusCode.Should().BeFalse();
        secondReopen.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
