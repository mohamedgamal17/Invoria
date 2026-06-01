using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Invoria.BuildingBlocks.Infrastructure.Common;
using Invoria.Ordering.Application.Orders.Commands.RecordOrderAllocationSucceeded;
using Invoria.Ordering.Contracts.Dtos;
using Invoria.Ordering.Contracts.Orders;
using Invoria.Ordering.Domain;
using Invoria.Ordering.Domain.Orders;
using Invoria.Ordering.Endpoints.Orders.Requests;
using Invoria.Ordering.Tests.Fakes;
using MediatR;
using Invoria.Ordering.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Ordering.Endpoints.Tests.Orders;

[TestFixture]
public class CancelOrderEndpointTests : OrderingTestFixture
{
    [Test]
    public async Task Should_cancel_order_when_pending()
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
        var cancelResponse = await Client.PostAsync(
            $"/orders/{created.Id}/cancel",
            emptyJson);

        cancelResponse.IsSuccessStatusCode.Should().BeTrue();
        cancelResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var cancelEnvelope = await cancelResponse.Content.ReadFromJsonAsync<Envelope<OrderDto>>();
        cancelEnvelope.Should().NotBeNull();
        cancelEnvelope!.Result!.Id.Should().Be(created.Id);
    }

    [Test]
    public async Task Should_cancel_order_when_reopened()
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

        // Accept via HTTP leaves fulfillment Allocating; Reopen then moves to Releasing, which is not cancellable.
        // Seed Accepted + Pending so Reopen transitions immediately to Reopened with OnHold (same as ReopenOrderCommandHandlerTests).
        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<OrderingDbContext>();
            var rows = await db.Set<Order>()
                .Where(o => o.Id == created.Id)
                .ExecuteUpdateAsync(s => s.SetProperty(o => o.Status, OrderStatus.Accepted));
            rows.Should().Be(1);
        }

        var reopenResponse = await Client.PostAsync(
            $"/orders/{created.Id}/reopen",
            new StringContent("{}", Encoding.UTF8, "application/json"));
        reopenResponse.IsSuccessStatusCode.Should().BeTrue();

        var cancelResponse = await Client.PostAsync(
            $"/orders/{created.Id}/cancel",
            new StringContent("{}", Encoding.UTF8, "application/json"));

        cancelResponse.IsSuccessStatusCode.Should().BeTrue();
        cancelResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var cancelEnvelope = await cancelResponse.Content.ReadFromJsonAsync<Envelope<OrderDto>>();
        cancelEnvelope.Should().NotBeNull();
        cancelEnvelope!.Result!.Id.Should().Be(created.Id);
    }

    [Test]
    public async Task Should_fail_when_order_cannot_be_cancelled()
    {
        var createRequest = new CreateOrderRequest
        {
            CustomerId = Guid.NewGuid().ToString(),
            Items =
            [
                new CreateOrderLineItemRequest
                    { ProductId = Guid.NewGuid().ToString(), Quantity = 1, Price = 1m }
            ]
        };

        var createResponse = await Client.PostAsJsonAsync("/orders", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var createEnvelope = await createResponse.Content.ReadFromJsonAsync<Envelope<OrderDto>>();
        var created = createEnvelope!.Result!;

        var emptyJson = new StringContent("{}", Encoding.UTF8, "application/json");
        var acceptResponse = await Client.PostAsync($"/orders/{created.Id}/accept", emptyJson);
        acceptResponse.EnsureSuccessStatusCode();

        var mediator = Scope.ServiceProvider.GetRequiredService<IMediator>();
        await mediator.Send(new RecordOrderAllocationSucceededCommand
        {
            OrderId = created.Id,
            CustomerId = created.CustomerId
        });

        var orderRepository = Scope.ServiceProvider.GetRequiredService<IOrderingRepository<Order>>();
        await OrderFulfillmentTestTransitions.DispatchAndShipAsync(orderRepository, created.Id);

        var cancelResponse = await Client.PostAsync(
            $"/orders/{created.Id}/cancel",
            new StringContent("{}", Encoding.UTF8, "application/json"));

        cancelResponse.IsSuccessStatusCode.Should().BeFalse();
        cancelResponse.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}

