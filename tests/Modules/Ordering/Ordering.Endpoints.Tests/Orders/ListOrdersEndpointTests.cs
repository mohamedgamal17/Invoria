using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Infrastructure.Common;
using Invoria.Ordering.Contracts.Dtos;
using Invoria.Ordering.Endpoints.Orders.Requests;
using Invoria.Endpoints.Tests.Utilities;

namespace Invoria.Ordering.Endpoints.Tests.Orders;

[TestFixture]
public class ListOrdersEndpointTests : OrderingTestFixture
{
    [Test]
    public async Task Should_return_paged_list_including_created_order()
    {
        var productId = Guid.NewGuid().ToString();
        var customerId = Guid.NewGuid().ToString();

        var createRequest = new CreateOrderRequest
        {
            CustomerId = customerId,
            Items =
            [
                new CreateOrderLineItemRequest { ProductId = productId, Quantity = 1, Price = 10m }
            ]
        };

        var createResponse = await Client.PostAsJsonAsync("/orders", createRequest);
        createResponse.IsSuccessStatusCode.Should().BeTrue();

        var createEnvelope = await createResponse.Content.ReadFromJsonAsync<Envelope<OrderDto>>();
        createEnvelope.Should().NotBeNull();
        var createdOrder = createEnvelope!.Result!;
        createdOrder.Id.Should().NotBeNullOrEmpty();

        var listQuery = new { Skip = 0, Length = 100, OrderNumber = createdOrder.OrderNumber };

        var uri = "/orders?" + QueryStringHelper.ToQueryString(listQuery);

        var response = await Client.GetAsync(uri);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task Should_map_include_order_items_query_parameter()
    {
        var productId = Guid.NewGuid().ToString();
        var customerId = Guid.NewGuid().ToString();

        var createRequest = new CreateOrderRequest
        {
            CustomerId = customerId,
            Items =
            [
                new CreateOrderLineItemRequest { ProductId = productId, Quantity = 3, Price = 12.5m }
            ]
        };

        var createResponse = await Client.PostAsJsonAsync("/orders", createRequest);
        createResponse.IsSuccessStatusCode.Should().BeTrue();

        var createEnvelope = await createResponse.Content.ReadFromJsonAsync<Envelope<OrderDto>>();
        var createdOrder = createEnvelope!.Result!;

        var listQuery = new { Skip = 0, Length = 100, IncludeOrderItems = true, OrderNumber = createdOrder.OrderNumber };
        var uri = "/orders?" + QueryStringHelper.ToQueryString(listQuery);

        var response = await Client.GetAsync(uri);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PagingDto<OrderDto>>>();

        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeTrue();
        var dto = envelope.Result!.Data.Single(x => x.Id == createdOrder.Id);
        dto.Items.Should().HaveCount(1);
        dto.Items[0].ProductId.Should().Be(productId);
        dto.Items[0].Quantity.Should().Be(3);
        dto.Items[0].Price.Should().Be(12.5m);
    }

    [Test]
    public async Task Should_map_order_number_prefix_filter()
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
        var createdOrder = createEnvelope!.Result!;
        // Full order number as StartsWith term — short prefixes can match many rows and omit the new order from the first page.
        var orderNumberPrefix = createdOrder.OrderNumber;

        var listQuery = new { Skip = 0, Length = 100, OrderNumber = orderNumberPrefix };
        var uri = "/orders?" + QueryStringHelper.ToQueryString(listQuery);

        var response = await Client.GetAsync(uri);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PagingDto<OrderDto>>>();

        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeTrue();
        envelope.Result!.Data.Should().Contain(x => x.Id == createdOrder.Id);
    }
}
