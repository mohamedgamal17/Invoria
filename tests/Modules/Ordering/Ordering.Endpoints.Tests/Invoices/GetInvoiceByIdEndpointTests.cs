using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Invoria.BuildingBlocks.Infrastructure.Common;
using Invoria.Ordering.Application.Invoices.Commands.CreateInvoice;
using Invoria.Ordering.Contracts.Invoices.Dtos;
using Invoria.Ordering.Contracts.Orders.Dtos;
using Invoria.Ordering.Endpoints.Orders.Requests;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Ordering.Endpoints.Tests.Invoices;

[TestFixture]
public class GetInvoiceByIdEndpointTests : OrderingTestFixture
{
    [Test]
    public async Task Should_return_invoice_when_found()
    {
        var (orderId, invoiceId, customerId) = await CreateCompletedOrderWithInvoiceAsync();

        var response = await Client.GetAsync("/invoices/" + invoiceId);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<InvoiceDto>>();

        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeTrue();
        envelope.Result.Should().NotBeNull();
        envelope.Result!.Id.Should().Be(invoiceId);
        envelope.Result.InvoiceNumber.Should().NotBeNullOrEmpty();
        envelope.Result.OrderId.Should().Be(orderId);
        envelope.Result.CustomerId.Should().Be(customerId);
        envelope.Result.Items.Should().NotBeEmpty();
    }

    [Test]
    public async Task Should_return_404_when_invoice_not_found()
    {
        var nonExistentId = Guid.NewGuid().ToString();

        var response = await Client.GetAsync("/invoices/" + nonExistentId);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<(string OrderId, string InvoiceId, string CustomerId)> CreateCompletedOrderWithInvoiceAsync()
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
        createResponse.EnsureSuccessStatusCode();

        var createdOrder = (await createResponse.Content.ReadFromJsonAsync<Envelope<OrderDto>>())!.Result!;

        var emptyJson = new StringContent("{}", Encoding.UTF8, "application/json");
        (await Client.PostAsync($"/orders/{createdOrder.Id}/accept", emptyJson)).EnsureSuccessStatusCode();
        (await Client.PostAsync($"/orders/{createdOrder.Id}/complete", emptyJson)).EnsureSuccessStatusCode();

        await using var scope = Factory.Services.CreateAsyncScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var createInvoiceResult = await mediator.Send(new CreateInvoiceCommand(createdOrder.Id));

        createInvoiceResult.IsSuccess.Should().BeTrue();

        return (createdOrder.Id, createInvoiceResult.Value!.Id, customerId);
    }
}
