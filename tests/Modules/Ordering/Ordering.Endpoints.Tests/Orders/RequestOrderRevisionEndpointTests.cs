using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Invoria.BuildingBlocks.Infrastructure.Common;
using Invoria.Ordering.Application.Orders.Commands.MarkOrderAllocated;
using Invoria.Ordering.Application.Orders.Commands.RecordOrderAllocation;
using Invoria.Ordering.Contracts.Orders.Dtos;
using Invoria.Ordering.Contracts.Orders.Enums;
using Invoria.Ordering.Endpoints.Orders.Requests;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Ordering.Endpoints.Tests.Orders;

[TestFixture]
public class RequestOrderRevisionEndpointTests : OrderingTestFixture
{
    [Test]
    public async Task Should_request_order_revision()
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

        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            (await mediator.Send(new RecordOrderAllocationCommand(created.Id!, "alloc-1"))).IsSuccess.Should().BeTrue();
            (await mediator.Send(new MarkOrderAllocatedCommand(created.Id!))).IsSuccess.Should().BeTrue();
        }

        var requestRevisionResponse = await Client.PostAsync(
            $"/orders/{created.Id}/request-revision",
            emptyJson);

        requestRevisionResponse.IsSuccessStatusCode.Should().BeTrue();
        requestRevisionResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var revisionEnvelope = await requestRevisionResponse.Content.ReadFromJsonAsync<Envelope<OrderDto>>();
        revisionEnvelope.Should().NotBeNull();
        revisionEnvelope!.Result!.Id.Should().Be(created.Id);
        revisionEnvelope.Result.Status.Should().Be(OrderStatus.RevisionPending);
    }

    [Test]
    public async Task Should_fail_when_order_cannot_request_revision()
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

        var requestRevisionResponse = await Client.PostAsync(
            $"/orders/{created.Id}/request-revision",
            emptyJson);

        requestRevisionResponse.IsSuccessStatusCode.Should().BeFalse();
        requestRevisionResponse.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
