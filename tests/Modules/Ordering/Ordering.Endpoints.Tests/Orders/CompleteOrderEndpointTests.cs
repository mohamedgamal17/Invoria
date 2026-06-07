using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Invoria.BuildingBlocks.Infrastructure.Common;
using Invoria.Ordering.Contracts.Orders.Dtos;
using Invoria.Ordering.Domain;
using Invoria.Ordering.Domain.Orders;
using Invoria.Ordering.Endpoints.Orders.Requests;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Ordering.Endpoints.Tests.Orders;

[TestFixture]
public class CompleteOrderEndpointTests : OrderingTestFixture
{
    [Test]
    public async Task Should_complete_order_after_accept_and_allocation()
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

        var completeResponse = await Client.PostAsync(
            $"/orders/{created.Id}/complete",
            emptyJson);

        completeResponse.IsSuccessStatusCode.Should().BeTrue();
        completeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var completeEnvelope = await completeResponse.Content.ReadFromJsonAsync<Envelope<OrderDto>>();
        completeEnvelope.Should().NotBeNull();
        completeEnvelope!.Result!.Id.Should().Be(created.Id);
        completeEnvelope.Result.ReturnItems.Should().BeEmpty();
    }

    [Test]
    public async Task Should_complete_with_explicit_empty_return_items()
    {
        var created = await CreateAndAcceptProcessingOrderAsync();

        var completeRequest = new CompleteOrderRequest
        {
            Id = created.Id,
            Items = []
        };

        var completeResponse = await Client.PostAsJsonAsync($"/orders/{created.Id}/complete", completeRequest);
        completeResponse.IsSuccessStatusCode.Should().BeTrue();

        var envelope = await completeResponse.Content.ReadFromJsonAsync<Envelope<OrderDto>>();
        envelope!.Result!.ReturnItems.Should().BeEmpty();
    }

    [Test]
    public async Task Should_fail_when_order_cannot_be_completed()
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
        var completeResponse = await Client.PostAsync(
            $"/orders/{created.Id}/complete",
            emptyJson);

        completeResponse.IsSuccessStatusCode.Should().BeFalse();
        completeResponse.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    public async Task Should_complete_with_return_items()
    {
        var createRequest = new CreateOrderRequest
        {
            CustomerId = Guid.NewGuid().ToString(),
            Items =
            [
                new CreateOrderLineItemRequest { ProductId = Guid.NewGuid().ToString(), Quantity = 2, Price = 10m }
            ]
        };

        var createResponse = await Client.PostAsJsonAsync("/orders", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<Envelope<OrderDto>>())!.Result!;

        var emptyJson = new StringContent("{}", Encoding.UTF8, "application/json");
        var acceptResponse = await Client.PostAsync($"/orders/{created.Id}/accept", emptyJson);
        acceptResponse.EnsureSuccessStatusCode();

        var getResponse = await Client.GetAsync($"/orders/{created.Id}");
        getResponse.EnsureSuccessStatusCode();
        var lineId = (await getResponse.Content.ReadFromJsonAsync<Envelope<OrderDto>>())!.Result!.Items.Single().Id;

        var completeRequest = new CompleteOrderRequest
        {
            Id = created.Id,
            Items = [new CompleteReturnLineItemRequest { OrderItemId = lineId, Quantity = 1 }]
        };

        var completeResponse = await Client.PostAsJsonAsync($"/orders/{created.Id}/complete", completeRequest);
        completeResponse.IsSuccessStatusCode.Should().BeTrue();

        var envelope = await completeResponse.Content.ReadFromJsonAsync<Envelope<OrderDto>>();
        envelope!.Result!.ReturnItems.Should().ContainSingle();
        envelope.Result.ReturnItems[0].OrderItemId.Should().Be(lineId);
        envelope.Result.NetOfTotalOrderAmount.Should().BeLessThan(envelope.Result.TotalOrderAmount);
    }

    [Test]
    public async Task Should_complete_with_multiple_return_items()
    {
        var createRequest = new CreateOrderRequest
        {
            CustomerId = Guid.NewGuid().ToString(),
            Items =
            [
                new CreateOrderLineItemRequest { ProductId = Guid.NewGuid().ToString(), Quantity = 2, Price = 10m },
                new CreateOrderLineItemRequest { ProductId = Guid.NewGuid().ToString(), Quantity = 1, Price = 5m }
            ]
        };

        var createResponse = await Client.PostAsJsonAsync("/orders", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<Envelope<OrderDto>>())!.Result!;

        var emptyJson = new StringContent("{}", Encoding.UTF8, "application/json");
        (await Client.PostAsync($"/orders/{created.Id}/accept", emptyJson)).EnsureSuccessStatusCode();

        var getResponse = await Client.GetAsync($"/orders/{created.Id}");
        getResponse.EnsureSuccessStatusCode();
        var lines = (await getResponse.Content.ReadFromJsonAsync<Envelope<OrderDto>>())!.Result!.Items;

        var completeRequest = new CompleteOrderRequest
        {
            Id = created.Id,
            Items =
            [
                new CompleteReturnLineItemRequest { OrderItemId = lines[0].Id, Quantity = 1 },
                new CompleteReturnLineItemRequest { OrderItemId = lines[1].Id, Quantity = 1 }
            ]
        };

        var completeResponse = await Client.PostAsJsonAsync($"/orders/{created.Id}/complete", completeRequest);
        completeResponse.IsSuccessStatusCode.Should().BeTrue();

        var envelope = await completeResponse.Content.ReadFromJsonAsync<Envelope<OrderDto>>();
        envelope!.Result!.ReturnItems.Should().HaveCount(2);
    }

    [Test]
    public async Task Should_complete_when_processing()
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
        var acceptResponse = await Client.PostAsync($"/orders/{created.Id}/accept", emptyJson);
        acceptResponse.EnsureSuccessStatusCode();

        var completeResponse = await Client.PostAsync($"/orders/{created.Id}/complete", emptyJson);
        completeResponse.IsSuccessStatusCode.Should().BeTrue();
    }

    private async Task<OrderDto> CreateAndAcceptProcessingOrderAsync()
    {
        var createRequest = new CreateOrderRequest
        {
            CustomerId = Guid.NewGuid().ToString(),
            Items =
            [
                new CreateOrderLineItemRequest { ProductId = Guid.NewGuid().ToString(), Quantity = 1, Price = 10m }
            ]
        };

        var createResponse = await Client.PostAsJsonAsync("/orders", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = (await createResponse.Content.ReadFromJsonAsync<Envelope<OrderDto>>())!.Result!;

        var emptyJson = new StringContent("{}", Encoding.UTF8, "application/json");
        (await Client.PostAsync($"/orders/{created.Id}/accept", emptyJson)).EnsureSuccessStatusCode();

        return created;
    }
}
