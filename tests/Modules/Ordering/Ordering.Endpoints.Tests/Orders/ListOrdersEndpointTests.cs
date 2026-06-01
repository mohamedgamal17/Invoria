using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Infrastructure.Common;
using Invoria.Ordering.Application.Orders.Commands.RecordOrderAllocationSucceeded;
using Invoria.Ordering.Contracts.Dtos;
using Invoria.Ordering.Contracts.Orders;
using Invoria.Ordering.Endpoints.Orders.Requests;
using Invoria.Endpoints.Tests.Utilities;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

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
    public async Task Should_map_include_return_items_query_parameter()
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
        var createdOrder = (await createResponse.Content.ReadFromJsonAsync<Envelope<OrderDto>>())!.Result!;

        var emptyJson = new StringContent("{}", Encoding.UTF8, "application/json");
        (await Client.PostAsync($"/orders/{createdOrder.Id}/accept", emptyJson)).EnsureSuccessStatusCode();

        var mediator = Scope.ServiceProvider.GetRequiredService<IMediator>();
        await mediator.Send(new RecordOrderAllocationSucceededCommand
        {
            OrderId = createdOrder.Id,
            CustomerId = createdOrder.CustomerId
        });

        (await Client.PostAsync($"/orders/{createdOrder.Id}/dispatch", emptyJson)).EnsureSuccessStatusCode();
        (await Client.PostAsync($"/orders/{createdOrder.Id}/ship", emptyJson)).EnsureSuccessStatusCode();

        var getResponse = await Client.GetAsync($"/orders/{createdOrder.Id}");
        getResponse.EnsureSuccessStatusCode();
        var lineId = (await getResponse.Content.ReadFromJsonAsync<Envelope<OrderDto>>())!.Result!.Items.Single().Id;

        var returnRequest = new AddReturnItemsRequest
        {
            Id = createdOrder.Id,
            Items = [new AddReturnLineItemRequest { OrderItemId = lineId, Quantity = 1 }]
        };
        (await Client.PutAsJsonAsync($"/orders/{createdOrder.Id}/return-items", returnRequest))
            .EnsureSuccessStatusCode();

        var listQuery = new
        {
            Skip = 0,
            Length = 100,
            IncludeReturnItems = true,
            OrderNumber = createdOrder.OrderNumber
        };
        var uri = "/orders?" + QueryStringHelper.ToQueryString(listQuery);

        var response = await Client.GetAsync(uri);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PagingDto<OrderDto>>>();
        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeTrue();

        var dto = envelope.Result!.Data.Single(x => x.Id == createdOrder.Id);
        dto.Items.Should().BeEmpty();
        dto.ReturnItems.Should().ContainSingle();
        dto.ReturnItems[0].OrderItemId.Should().Be(lineId);
        dto.NetOfTotalOrderAmount.Should().BeLessThan(dto.TotalOrderAmount);
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

    [Test]
    public async Task Should_map_customer_id_filter()
    {
        var productId = Guid.NewGuid().ToString();
        var targetCustomerId = Guid.NewGuid().ToString();
        var otherCustomerId = Guid.NewGuid().ToString();

        var targetCreateRequest = new CreateOrderRequest
        {
            CustomerId = targetCustomerId,
            Items =
            [
                new CreateOrderLineItemRequest { ProductId = productId, Quantity = 2, Price = 7.5m }
            ]
        };

        var otherCreateRequest = new CreateOrderRequest
        {
            CustomerId = otherCustomerId,
            Items =
            [
                new CreateOrderLineItemRequest { ProductId = productId, Quantity = 1, Price = 5m }
            ]
        };

        var targetCreateResponse = await Client.PostAsJsonAsync("/orders", targetCreateRequest);
        targetCreateResponse.IsSuccessStatusCode.Should().BeTrue();
        var targetEnvelope = await targetCreateResponse.Content.ReadFromJsonAsync<Envelope<OrderDto>>();
        var targetOrder = targetEnvelope!.Result!;

        var otherCreateResponse = await Client.PostAsJsonAsync("/orders", otherCreateRequest);
        otherCreateResponse.IsSuccessStatusCode.Should().BeTrue();

        var listQuery = new { Skip = 0, Length = 100, CustomerId = targetCustomerId };
        var uri = "/orders?" + QueryStringHelper.ToQueryString(listQuery);

        var response = await Client.GetAsync(uri);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PagingDto<OrderDto>>>();
        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeTrue();
        envelope.Result!.Data.Should().ContainSingle(x => x.Id == targetOrder.Id);
        envelope.Result.Data.Should().OnlyContain(x => x.CustomerId == targetCustomerId);
    }

    [Test]
    public async Task Should_return_validation_errors_envelope_when_request_is_invalid()
    {
        var listQuery = new { Skip = 0, Length = 0 };

        var uri = "/orders?" + QueryStringHelper.ToQueryString(listQuery);

        var response = await Client.GetAsync(uri);

        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope>();

        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeFalse();
        envelope.Error.Should().NotBeNull();
        envelope.Error!.Status.Should().Be((int)HttpStatusCode.BadRequest);
        envelope.Error.Errors.Should().NotBeEmpty();
        envelope.Error.Errors.Keys.Should().Contain(x => x.Contains("Length") || x == "GeneralErrors");
    }

    [Test]
    public async Task Should_map_order_status_filter()
    {
        var productId = Guid.NewGuid().ToString();
        var customerId = Guid.NewGuid().ToString();

        var pendingRequest = new CreateOrderRequest
        {
            CustomerId = customerId,
            Items = [new CreateOrderLineItemRequest { ProductId = productId, Quantity = 1, Price = 10m }]
        };

        var toAcceptRequest = new CreateOrderRequest
        {
            CustomerId = customerId,
            Items = [new CreateOrderLineItemRequest { ProductId = productId, Quantity = 1, Price = 10m }]
        };

        var pendingResponse = await Client.PostAsJsonAsync("/orders", pendingRequest);
        pendingResponse.IsSuccessStatusCode.Should().BeTrue();
        var pendingEnvelope = await pendingResponse.Content.ReadFromJsonAsync<Envelope<OrderDto>>();
        var pendingOrder = pendingEnvelope!.Result!;

        var toAcceptResponse = await Client.PostAsJsonAsync("/orders", toAcceptRequest);
        toAcceptResponse.IsSuccessStatusCode.Should().BeTrue();
        var toAcceptEnvelope = await toAcceptResponse.Content.ReadFromJsonAsync<Envelope<OrderDto>>();
        var toAcceptOrder = toAcceptEnvelope!.Result!;

        var acceptResponse = await Client.PostAsJsonAsync($"/orders/{toAcceptOrder.Id}/accept", new { });
        acceptResponse.IsSuccessStatusCode.Should().BeTrue();

        var listQuery = new { Skip = 0, Length = 100, Status = OrderStatus.Pending };
        var uri = "/orders?" + QueryStringHelper.ToQueryString(listQuery);

        var response = await Client.GetAsync(uri);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PagingDto<OrderDto>>>();
        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeTrue();
        envelope.Result!.Data.Should().Contain(x => x.Id == pendingOrder.Id);
        envelope.Result.Data.Should().NotContain(x => x.Id == toAcceptOrder.Id);
        envelope.Result.Data.Should().OnlyContain(x => x.Status == OrderStatus.Pending);
    }

    [Test]
    public async Task Should_map_fullfillment_status_filter()
    {
        var productId = Guid.NewGuid().ToString();
        var customerId = Guid.NewGuid().ToString();

        var pendingFulfillmentRequest = new CreateOrderRequest
        {
            CustomerId = customerId,
            Items = [new CreateOrderLineItemRequest { ProductId = productId, Quantity = 1, Price = 10m }]
        };

        var toAcceptRequest = new CreateOrderRequest
        {
            CustomerId = customerId,
            Items = [new CreateOrderLineItemRequest { ProductId = productId, Quantity = 1, Price = 10m }]
        };

        var pendingResponse = await Client.PostAsJsonAsync("/orders", pendingFulfillmentRequest);
        pendingResponse.IsSuccessStatusCode.Should().BeTrue();
        var pendingEnvelope = await pendingResponse.Content.ReadFromJsonAsync<Envelope<OrderDto>>();
        var pendingFulfillmentOrder = pendingEnvelope!.Result!;

        var toAcceptResponse = await Client.PostAsJsonAsync("/orders", toAcceptRequest);
        toAcceptResponse.IsSuccessStatusCode.Should().BeTrue();
        var toAcceptEnvelope = await toAcceptResponse.Content.ReadFromJsonAsync<Envelope<OrderDto>>();
        var toAcceptOrder = toAcceptEnvelope!.Result!;

        var acceptResponse = await Client.PostAsJsonAsync($"/orders/{toAcceptOrder.Id}/accept", new { });
        acceptResponse.IsSuccessStatusCode.Should().BeTrue();

        var listQuery = new { Skip = 0, Length = 100, FullfillmentStatus = FullfillmentStatus.Pending };
        var uri = "/orders?" + QueryStringHelper.ToQueryString(listQuery);

        var response = await Client.GetAsync(uri);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PagingDto<OrderDto>>>();
        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeTrue();
        envelope.Result!.Data.Should().Contain(x => x.Id == pendingFulfillmentOrder.Id);
        envelope.Result.Data.Should().NotContain(x => x.Id == toAcceptOrder.Id);
        envelope.Result.Data.Should().OnlyContain(x => x.FullfillmentStatus == FullfillmentStatus.Pending);
    }

    [Test]
    public async Task Should_map_payment_type_filter()
    {
        var productId = Guid.NewGuid().ToString();
        var customerId = Guid.NewGuid().ToString();

        var immediateRequest = new CreateOrderRequest
        {
            CustomerId = customerId,
            PaymentType = OrderPaymentType.Immediate,
            Items = [new CreateOrderLineItemRequest { ProductId = productId, Quantity = 1, Price = 10m }]
        };

        var debtRequest = new CreateOrderRequest
        {
            CustomerId = customerId,
            PaymentType = OrderPaymentType.Debt,
            Items = [new CreateOrderLineItemRequest { ProductId = productId, Quantity = 1, Price = 10m }]
        };

        var immediateResponse = await Client.PostAsJsonAsync("/orders", immediateRequest);
        immediateResponse.IsSuccessStatusCode.Should().BeTrue();
        var immediateEnvelope = await immediateResponse.Content.ReadFromJsonAsync<Envelope<OrderDto>>();
        var immediateOrder = immediateEnvelope!.Result!;

        var debtResponse = await Client.PostAsJsonAsync("/orders", debtRequest);
        debtResponse.IsSuccessStatusCode.Should().BeTrue();
        var debtEnvelope = await debtResponse.Content.ReadFromJsonAsync<Envelope<OrderDto>>();
        var debtOrder = debtEnvelope!.Result!;

        var listQuery = new { Skip = 0, Length = 100, PaymentType = OrderPaymentType.Debt };
        var uri = "/orders?" + QueryStringHelper.ToQueryString(listQuery);

        var response = await Client.GetAsync(uri);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PagingDto<OrderDto>>>();
        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeTrue();
        envelope.Result!.Data.Should().ContainSingle(x => x.Id == debtOrder.Id);
        envelope.Result.Data.Should().NotContain(x => x.Id == immediateOrder.Id);
        envelope.Result.Data.Should().OnlyContain(x => x.PaymentType == OrderPaymentType.Debt);
    }

    [Test]
    public async Task Should_map_payment_status_filter()
    {
        var productId = Guid.NewGuid().ToString();
        var customerId = Guid.NewGuid().ToString();

        var unpaidRequest = new CreateOrderRequest
        {
            CustomerId = customerId,
            PaymentType = OrderPaymentType.Debt,
            Items = [new CreateOrderLineItemRequest { ProductId = productId, Quantity = 2, Price = 10m }]
        };

        var unpaidResponse = await Client.PostAsJsonAsync("/orders", unpaidRequest);
        unpaidResponse.IsSuccessStatusCode.Should().BeTrue();
        var unpaidEnvelope = await unpaidResponse.Content.ReadFromJsonAsync<Envelope<OrderDto>>();
        var unpaidOrder = unpaidEnvelope!.Result!;

        var partialRequest = new CreateOrderRequest
        {
            CustomerId = customerId,
            PaymentType = OrderPaymentType.Debt,
            Items = [new CreateOrderLineItemRequest { ProductId = productId, Quantity = 2, Price = 10m }]
        };

        var partialResponse = await Client.PostAsJsonAsync("/orders", partialRequest);
        partialResponse.IsSuccessStatusCode.Should().BeTrue();
        var partialEnvelope = await partialResponse.Content.ReadFromJsonAsync<Envelope<OrderDto>>();
        var partialOrder = partialEnvelope!.Result!;

        var acceptResponse = await Client.PostAsJsonAsync($"/orders/{partialOrder.Id}/accept", new { });
        acceptResponse.IsSuccessStatusCode.Should().BeTrue();

        var mediator = Scope.ServiceProvider.GetRequiredService<IMediator>();
        await mediator.Send(new RecordOrderAllocationSucceededCommand
        {
            OrderId = partialOrder.Id,
            CustomerId = customerId
        });

        var dispatchResponse = await Client.PostAsJsonAsync($"/orders/{partialOrder.Id}/dispatch", new { Id = partialOrder.Id });
        dispatchResponse.IsSuccessStatusCode.Should().BeTrue();

        var emptyJson = new StringContent("{}", Encoding.UTF8, "application/json");
        (await Client.PostAsync($"/orders/{partialOrder.Id}/ship", emptyJson)).EnsureSuccessStatusCode();

        var completeResponse = await Client.PostAsJsonAsync($"/orders/{partialOrder.Id}/complete", new { });
        completeResponse.IsSuccessStatusCode.Should().BeTrue();

        var paymentResponse = await Client.PostAsJsonAsync($"/orders/{partialOrder.Id}/payments", new
        {
            PaidAmount = 10m,
            PaymentMethod = OrderPaymentMethod.Cash
        });
        paymentResponse.IsSuccessStatusCode.Should().BeTrue();

        var listQuery = new { Skip = 0, Length = 100, PaymentStatus = OrderPaymentStatus.Partial };
        var uri = "/orders?" + QueryStringHelper.ToQueryString(listQuery);

        var response = await Client.GetAsync(uri);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PagingDto<OrderDto>>>();
        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeTrue();
        envelope.Result!.Data.Should().ContainSingle(x => x.Id == partialOrder.Id);
        envelope.Result.Data.Should().NotContain(x => x.Id == unpaidOrder.Id);
        envelope.Result.Data.Should().OnlyContain(x => x.PaymentStatus == OrderPaymentStatus.Partial);
    }
}
