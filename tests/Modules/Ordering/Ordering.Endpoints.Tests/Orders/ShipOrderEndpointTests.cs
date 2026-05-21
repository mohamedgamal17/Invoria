using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Invoria.BuildingBlocks.Infrastructure.Common;
using Invoria.Ordering.Application.Orders.Commands.RecordOrderAllocationSucceeded;
using Invoria.Ordering.Contracts.Dtos;
using Invoria.Ordering.Contracts.Orders;
using Invoria.Ordering.Endpoints.Orders.Requests;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Ordering.Endpoints.Tests.Orders;

[TestFixture]
public class ShipOrderEndpointTests : OrderingTestFixture
{
    [Test]
    public async Task Should_ship_after_accept_allocation_and_dispatch()
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

        var dispatchResponse = await Client.PostAsync($"/orders/{created.Id}/dispatch", emptyJson);
        dispatchResponse.EnsureSuccessStatusCode();

        var shipResponse = await Client.PostAsync(
            $"/orders/{created.Id}/ship",
            emptyJson);

        shipResponse.IsSuccessStatusCode.Should().BeTrue();
        shipResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var shipEnvelope = await shipResponse.Content.ReadFromJsonAsync<Envelope<OrderDto>>();
        shipEnvelope.Should().NotBeNull();
        shipEnvelope!.Result!.Id.Should().Be(created.Id);
        shipEnvelope.Result!.Status.Should().Be(OrderStatus.Shipped);
    }

    [Test]
    public async Task Should_fail_when_order_cannot_be_shipped()
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

        var mediator = Scope.ServiceProvider.GetRequiredService<IMediator>();
        await mediator.Send(new RecordOrderAllocationSucceededCommand
        {
            OrderId = created.Id,
            CustomerId = created.CustomerId
        });

        var shipResponse = await Client.PostAsync(
            $"/orders/{created.Id}/ship",
            new StringContent("{}", Encoding.UTF8, "application/json"));

        shipResponse.IsSuccessStatusCode.Should().BeFalse();
        shipResponse.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
