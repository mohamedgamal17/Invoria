using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Invoria.BuildingBlocks.Infrastructure.Common;
using Invoria.Ordering.Contracts.Dtos;
using Invoria.Ordering.Domain;
using Invoria.Ordering.Domain.Orders;
using Invoria.Ordering.Endpoints.Orders.Requests;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Ordering.Endpoints.Tests.Orders;

[TestFixture]
public class AddReturnItemsEndpointTests : OrderingTestFixture
{
    private async Task<OrderDto> CreateAndPrepareProcessingOrderAsync()
    {
        var createRequest = new CreateOrderRequest
        {
            CustomerId = Guid.NewGuid().ToString(),
            Items =
            [
                new CreateOrderLineItemRequest
                {
                    ProductId = Guid.NewGuid().ToString(),
                    Quantity = 2,
                    Price = 10m
                }
            ]
        };

        var createResponse = await Client.PostAsJsonAsync("/orders", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var createEnvelope = await createResponse.Content.ReadFromJsonAsync<Envelope<OrderDto>>();
        var created = createEnvelope!.Result!;

        var emptyJson = new StringContent("{}", Encoding.UTF8, "application/json");
        var acceptResponse = await Client.PostAsync($"/orders/{created.Id}/accept", emptyJson);
        acceptResponse.EnsureSuccessStatusCode();

        return created;
    }

    [Test]
    public async Task Should_record_return_items_after_accept()
    {
        var created = await CreateAndPrepareProcessingOrderAsync();

        var getResponse = await Client.GetAsync($"/orders/{created.Id}");
        getResponse.EnsureSuccessStatusCode();
        var getEnvelope = await getResponse.Content.ReadFromJsonAsync<Envelope<OrderDto>>();
        var lineId = getEnvelope!.Result!.Items.Single().Id;
        lineId.Should().NotBeNullOrEmpty();

        var returnRequest = new AddReturnItemsRequest
        {
            Id = created.Id,
            Items = [new AddReturnLineItemRequest { OrderItemId = lineId, Quantity = 1 }]
        };

        var response = await Client.PutAsJsonAsync(
            $"/orders/{created.Id}/return-items",
            returnRequest);

        response.IsSuccessStatusCode.Should().BeTrue();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await response.Content.ReadFromJsonAsync<Envelope<OrderDto>>();
        envelope.Should().NotBeNull();
        envelope!.Result!.Id.Should().Be(created.Id);
        envelope.Result.ReturnItems.Should().ContainSingle();
        envelope.Result.ReturnItems[0].OrderItemId.Should().Be(lineId);
        envelope.Result.ReturnItems[0].Quantity.Should().Be(1);
        envelope.Result.NetOfTotalOrderAmount.Should().BeLessThan(envelope.Result.TotalOrderAmount);
        envelope.Result.ReturnsTotal.Should().BeGreaterThan(0);
    }

    [Test]
    public async Task Should_fail_when_request_invalid()
    {
        var orderId = Guid.NewGuid().ToString();

        var response = await Client.PutAsJsonAsync(
            $"/orders/{orderId}/return-items",
            new AddReturnItemsRequest
            {
                Id = orderId,
                Items = [new AddReturnLineItemRequest { OrderItemId = "", Quantity = 0 }]
            });

        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope>();
        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeFalse();
        envelope.Error.Should().NotBeNull();
        envelope.Error!.Status.Should().Be((int)HttpStatusCode.BadRequest);
        envelope.Error.Errors.Should().NotBeEmpty();
    }

    [Test]
    public async Task Should_record_return_items_when_order_pending()
    {
        var createRequest = new CreateOrderRequest
        {
            CustomerId = Guid.NewGuid().ToString(),
            Items =
            [
                new CreateOrderLineItemRequest
                {
                    ProductId = Guid.NewGuid().ToString(),
                    Quantity = 1,
                    Price = 1m
                }
            ]
        };

        var createResponse = await Client.PostAsJsonAsync("/orders", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var createEnvelope = await createResponse.Content.ReadFromJsonAsync<Envelope<OrderDto>>();
        var created = createEnvelope!.Result!;

        var getResponse = await Client.GetAsync($"/orders/{created.Id}");
        getResponse.EnsureSuccessStatusCode();
        var getEnvelope = await getResponse.Content.ReadFromJsonAsync<Envelope<OrderDto>>();
        var lineId = getEnvelope!.Result!.Items.Single().Id;

        var response = await Client.PutAsJsonAsync(
            $"/orders/{created.Id}/return-items",
            new AddReturnItemsRequest
            {
                Id = created.Id,
                Items = [new AddReturnLineItemRequest { OrderItemId = lineId, Quantity = 1 }]
            });

        response.IsSuccessStatusCode.Should().BeTrue();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
