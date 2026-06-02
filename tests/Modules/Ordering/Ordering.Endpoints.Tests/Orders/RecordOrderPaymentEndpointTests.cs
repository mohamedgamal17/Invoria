using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Invoria.BuildingBlocks.Infrastructure.Common;
using Invoria.Ordering.Contracts.Dtos;
using Invoria.Ordering.Contracts.Orders;
using Invoria.Ordering.Domain;
using Invoria.Ordering.Domain.Orders;
using Invoria.Ordering.Endpoints.Orders.Requests;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Ordering.Endpoints.Tests.Orders;

[TestFixture]
public class RecordOrderPaymentEndpointTests : OrderingTestFixture
{
    private static decimal SumLines(CreateOrderRequest request) =>
        request.Items.Sum(i => i.Quantity * i.Price);

    /// <summary>HTTP create → accept → allocation succeeded (mediator) → dispatch/ship (repository) → complete; returns finalized order.</summary>
    private async Task<OrderDto> CreateAndFullyCompleteOrderAsync(CreateOrderRequest createRequest)
    {
        var createResponse = await Client.PostAsJsonAsync("/orders", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var createEnvelope = await createResponse.Content.ReadFromJsonAsync<Envelope<OrderDto>>();
        createEnvelope.Should().NotBeNull();
        var created = createEnvelope!.Result!;
        created.Id.Should().NotBeNullOrEmpty();

        var emptyJson = new StringContent("{}", Encoding.UTF8, "application/json");

        var acceptResponse = await Client.PostAsync($"/orders/{created.Id}/accept", emptyJson);
        acceptResponse.EnsureSuccessStatusCode();

        var completeResponse = await Client.PostAsync($"/orders/{created.Id}/complete", emptyJson);
        completeResponse.EnsureSuccessStatusCode();
        var completeEnvelope = await completeResponse.Content.ReadFromJsonAsync<Envelope<OrderDto>>();
        completeEnvelope.Should().NotBeNull();
        completeEnvelope!.Result!.Id.Should().Be(created.Id);
        return completeEnvelope.Result!;
    }

    [Test]
    public async Task Should_record_full_payment_when_order_completed_and_immediate()
    {
        var productId = Guid.NewGuid().ToString();
        var createRequest = new CreateOrderRequest
        {
            CustomerId = Guid.NewGuid().ToString(),
            Items =
            [
                new CreateOrderLineItemRequest { ProductId = productId, Quantity = 2, Price = 10m }
            ]
        };

        var completed = await CreateAndFullyCompleteOrderAsync(createRequest);
        var total = SumLines(createRequest);

        var paymentResponse = await Client.PostAsJsonAsync(
            $"/orders/{completed.Id}/payments",
            new { PaidAmount = total, PaymentMethod = OrderPaymentMethod.BankTransfer });

        paymentResponse.IsSuccessStatusCode.Should().BeTrue();
        paymentResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var paymentEnvelope = await paymentResponse.Content.ReadFromJsonAsync<Envelope<OrderDto>>();
        paymentEnvelope.Should().NotBeNull();
        paymentEnvelope!.IsSuccess.Should().BeTrue();
        var dto = paymentEnvelope.Result!;
        dto.PaymentStatus.Should().Be(OrderPaymentStatus.Paid);
        dto.AmountOutstanding.Should().Be(0);
        dto.AmountPaid.Should().Be(total);
        dto.Payments.Should().ContainSingle();
    }

    [Test]
    public async Task Should_record_partial_payment_when_order_completed_and_debt()
    {
        var productId = Guid.NewGuid().ToString();
        var createRequest = new CreateOrderRequest
        {
            CustomerId = Guid.NewGuid().ToString(),
            PaymentType = OrderPaymentType.Debt,
            Items =
            [
                new CreateOrderLineItemRequest { ProductId = productId, Quantity = 2, Price = 10m }
            ]
        };

        var completed = await CreateAndFullyCompleteOrderAsync(createRequest);
        var total = SumLines(createRequest);
        total.Should().BeGreaterThan(1m);

        var paymentResponse = await Client.PostAsJsonAsync(
            $"/orders/{completed.Id}/payments",
            new { PaidAmount = 1m, PaymentMethod = OrderPaymentMethod.Cash });

        paymentResponse.IsSuccessStatusCode.Should().BeTrue();
        paymentResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var paymentEnvelope = await paymentResponse.Content.ReadFromJsonAsync<Envelope<OrderDto>>();
        paymentEnvelope.Should().NotBeNull();
        paymentEnvelope!.IsSuccess.Should().BeTrue();
        var dto = paymentEnvelope.Result!;
        dto.PaymentStatus.Should().Be(OrderPaymentStatus.Partial);
        dto.AmountPaid.Should().Be(1m);
        dto.AmountOutstanding.Should().Be(total - 1m);
        dto.Payments.Should().ContainSingle();
    }

    [Test]
    public async Task Should_fail_when_paid_amount_invalid()
    {
        var orderId = Guid.NewGuid().ToString();

        var paymentResponse = await Client.PostAsJsonAsync(
            $"/orders/{orderId}/payments",
            new { PaidAmount = 0m, PaymentMethod = OrderPaymentMethod.Cash });

        paymentResponse.IsSuccessStatusCode.Should().BeFalse();
        paymentResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var envelope = await paymentResponse.Content.ReadFromJsonAsync<Envelope>();
        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeFalse();
        envelope.Error.Should().NotBeNull();
        envelope.Error!.Status.Should().Be((int)HttpStatusCode.BadRequest);
        envelope.Error.Errors.Should().NotBeEmpty();
    }

    [Test]
    public async Task Should_fail_when_order_not_completed()
    {
        var createRequest = new CreateOrderRequest
        {
            CustomerId = Guid.NewGuid().ToString(),
            Items =
            [
                new CreateOrderLineItemRequest { ProductId = Guid.NewGuid().ToString(), Quantity = 1, Price = 5m }
            ]
        };

        var createResponse = await Client.PostAsJsonAsync("/orders", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var createEnvelope = await createResponse.Content.ReadFromJsonAsync<Envelope<OrderDto>>();
        var created = createEnvelope!.Result!;

        var emptyJson = new StringContent("{}", Encoding.UTF8, "application/json");
        var acceptResponse = await Client.PostAsync($"/orders/{created.Id}/accept", emptyJson);
        acceptResponse.EnsureSuccessStatusCode();

        var paymentResponse = await Client.PostAsJsonAsync(
            $"/orders/{created.Id}/payments",
            new { PaidAmount = 1m, PaymentMethod = OrderPaymentMethod.Cash });

        paymentResponse.IsSuccessStatusCode.Should().BeFalse();
        paymentResponse.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    public async Task Should_fail_when_order_not_found()
    {
        var nonExistentId = Guid.NewGuid().ToString();

        var paymentResponse = await Client.PostAsJsonAsync(
            $"/orders/{nonExistentId}/payments",
            new { PaidAmount = 1m, PaymentMethod = OrderPaymentMethod.Cash });

        paymentResponse.IsSuccessStatusCode.Should().BeFalse();
        paymentResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
