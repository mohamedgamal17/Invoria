using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Invoria.BuildingBlocks.Infrastructure.Common;
using Invoria.Ordering.Application.Orders.Commands.RecordOrderAllocationSucceeded;
using Invoria.Ordering.Contracts.Dtos;
using Invoria.Ordering.Endpoints.Orders.Requests;
using MediatR;

namespace Invoria.Ordering.Endpoints.Tests.Orders;

[TestFixture]
public class DispatchOrderEndpointTests : OrderingTestFixture
{
    [Test]
    public async Task Should_dispatch_after_accept_and_allocation()
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
            CustomerId = customerId
        });

        var dispatchResponse = await Client.PostAsync(
            $"/orders/{created.Id}/dispatch",
            emptyJson);

        dispatchResponse.IsSuccessStatusCode.Should().BeTrue();
        dispatchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var dispatchEnvelope = await dispatchResponse.Content.ReadFromJsonAsync<Envelope<OrderDto>>();
        dispatchEnvelope.Should().NotBeNull();
        dispatchEnvelope!.Result!.Id.Should().Be(created.Id);
    }

    [Test]
    public async Task Should_fail_when_order_cannot_be_dispatched()
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

        var acceptResponse = await Client.PostAsync(
            $"/orders/{created.Id}/accept",
            new StringContent("{}", Encoding.UTF8, "application/json"));
        acceptResponse.EnsureSuccessStatusCode();

        var dispatchResponse = await Client.PostAsync(
            $"/orders/{created.Id}/dispatch",
            new StringContent("{}", Encoding.UTF8, "application/json"));

        dispatchResponse.IsSuccessStatusCode.Should().BeFalse();
        dispatchResponse.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
