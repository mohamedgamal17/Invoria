using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Invoria.BuildingBlocks.Infrastructure.Common;
using Invoria.Ordering.Application.Orders.Commands.RecordOrderAllocationSucceeded;
using Invoria.Ordering.Contracts.Dtos;
using Invoria.Ordering.Endpoints.Orders.Requests;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Ordering.Endpoints.Tests.Orders;

[TestFixture]
public class RefuseOrderEndpointTests : OrderingTestFixture
{
    [Test]
    public async Task Should_refuse_order_when_accepted()
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
        var acceptResponse = await Client.PostAsync(
            $"/orders/{created.Id}/accept",
            emptyJson);
        acceptResponse.EnsureSuccessStatusCode();

        var refuseResponse = await Client.PostAsync(
            $"/orders/{created.Id}/refuse",
            emptyJson);

        refuseResponse.IsSuccessStatusCode.Should().BeTrue();
        refuseResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var refuseEnvelope = await refuseResponse.Content.ReadFromJsonAsync<Envelope<OrderDto>>();
        refuseEnvelope.Should().NotBeNull();
        refuseEnvelope!.Result!.Id.Should().Be(created.Id);
    }

    [Test]
    public async Task Should_refuse_order_when_completed()
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

        var mediator = Scope.ServiceProvider.GetRequiredService<IMediator>();
        await mediator.Send(new RecordOrderAllocationSucceededCommand
        {
            OrderId = created.Id,
            CustomerId = created.CustomerId
        });

        var dispatchResponse = await Client.PostAsync($"/orders/{created.Id}/dispatch", emptyJson);
        dispatchResponse.EnsureSuccessStatusCode();

        var shipResponse = await Client.PostAsync($"/orders/{created.Id}/ship", emptyJson);
        shipResponse.EnsureSuccessStatusCode();

        var completeResponse = await Client.PostAsync(
            $"/orders/{created.Id}/complete",
            emptyJson);
        completeResponse.EnsureSuccessStatusCode();

        var refuseResponse = await Client.PostAsync(
            $"/orders/{created.Id}/refuse",
            emptyJson);

        refuseResponse.IsSuccessStatusCode.Should().BeTrue();
        refuseResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var refuseEnvelope = await refuseResponse.Content.ReadFromJsonAsync<Envelope<OrderDto>>();
        refuseEnvelope.Should().NotBeNull();
        refuseEnvelope!.Result!.Id.Should().Be(created.Id);
    }

    [Test]
    public async Task Should_fail_when_order_cannot_be_refused()
    {
        var productId = Guid.NewGuid().ToString();
        var customerId = Guid.NewGuid().ToString();

        var createRequest = new CreateOrderRequest
        {
            CustomerId = customerId,
            Items =
            [
                new CreateOrderLineItemRequest { ProductId = productId, Quantity = 1, Price = 1m }
            ]
        };

        var createResponse = await Client.PostAsJsonAsync("/orders", createRequest);
        createResponse.IsSuccessStatusCode.Should().BeTrue();
        var createEnvelope = await createResponse.Content.ReadFromJsonAsync<Envelope<OrderDto>>();
        createEnvelope.Should().NotBeNull();
        var created = createEnvelope!.Result!;
        created.Id.Should().NotBeNullOrEmpty();

        var emptyJson = new StringContent("{}", Encoding.UTF8, "application/json");
        var refuseResponse = await Client.PostAsync(
            $"/orders/{created.Id}/refuse",
            emptyJson);

        refuseResponse.IsSuccessStatusCode.Should().BeFalse();
        refuseResponse.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}

