using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Invoria.BuildingBlocks.Infrastructure.Common;
using Invoria.Ordering.Contracts.Orders.Dtos;
using Invoria.Ordering.Endpoints.Orders.Requests;

namespace Invoria.Ordering.Endpoints.Tests.Orders;

[TestFixture]
public class GetOrderByIdEndpointTests : OrderingTestFixture
{
    [Test]
    public async Task Should_return_order_when_found()
    {
        var productId = Guid.NewGuid().ToString();
        var customerId = Guid.NewGuid().ToString();

        var createRequest = new CreateOrderRequest
        {
            CustomerId = customerId,
            Items =
            [
                new CreateOrderLineItemRequest { ProductId = productId, Quantity = 2, Price = 15m }
            ]
        };

        var createResponse = await Client.PostAsJsonAsync("/orders", createRequest);
        createResponse.IsSuccessStatusCode.Should().BeTrue();

        var createEnvelope = await createResponse.Content.ReadFromJsonAsync<Envelope<OrderDto>>();
        createEnvelope.Should().NotBeNull();
        var createdOrder = createEnvelope!.Result!;
        createdOrder.Id.Should().NotBeNullOrEmpty();

        var response = await Client.GetAsync("/orders/" + createdOrder.Id);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<OrderDto>>();

        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeTrue();
        envelope.Result.Should().NotBeNull();
        envelope.Result!.Id.Should().Be(createdOrder.Id);
        envelope.Result.OrderNumber.Should().Be(createdOrder.OrderNumber);
        envelope.Result.CustomerId.Should().Be(customerId);
        envelope.Result.Customer.Should().BeNull();
        envelope.Result.Items.Should().HaveCount(1);
        envelope.Result.Items[0].ProductId.Should().Be(productId);
        envelope.Result.Items[0].Quantity.Should().Be(2);
        envelope.Result.Items[0].Price.Should().Be(15m);
    }

    [Test]
    public async Task Should_return_404_when_order_not_found()
    {
        var nonExistentId = Guid.NewGuid().ToString();

        var response = await Client.GetAsync("/orders/" + nonExistentId);

        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
