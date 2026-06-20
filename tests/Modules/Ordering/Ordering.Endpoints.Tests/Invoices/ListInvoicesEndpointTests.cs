using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Infrastructure.Common;
using Invoria.Ordering.Application.Invoices.Commands.CreateInvoice;
using Invoria.Ordering.Contracts.Invoices.Dtos;
using Invoria.Ordering.Contracts.Orders.Dtos;
using Invoria.Ordering.Endpoints.Orders.Requests;
using Invoria.Endpoints.Tests.Utilities;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Ordering.Endpoints.Tests.Invoices;

[TestFixture]
public class ListInvoicesEndpointTests : OrderingTestFixture
{
    [Test]
    public async Task Should_return_paged_list_including_created_invoice()
    {
        var (orderId, invoiceId) = await CreateCompletedOrderWithInvoiceAsync();

        var listQuery = new { Skip = 0, Length = 100 };
        var uri = "/invoices?" + QueryStringHelper.ToQueryString(listQuery);

        var response = await Client.GetAsync(uri);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PagingDto<InvoiceDto>>>();

        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeTrue();
        envelope.Result!.Data.Should().ContainSingle(i => i.Id == invoiceId && i.OrderId == orderId);
        envelope.Result.Data.Single(i => i.Id == invoiceId).Items.Should().NotBeEmpty();
    }

    [Test]
    public async Task Should_filter_by_order_id_query_parameter()
    {
        var (orderId, invoiceId) = await CreateCompletedOrderWithInvoiceAsync();
        await CreateCompletedOrderWithInvoiceAsync();

        var listQuery = new { Skip = 0, Length = 100, OrderId = orderId };
        var uri = "/invoices?" + QueryStringHelper.ToQueryString(listQuery);

        var response = await Client.GetAsync(uri);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PagingDto<InvoiceDto>>>();

        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeTrue();
        envelope.Result!.Data.Should().ContainSingle();
        envelope.Result.Data.Single().Id.Should().Be(invoiceId);
        envelope.Result.Data.Single().OrderId.Should().Be(orderId);
    }

    private async Task<(string OrderId, string InvoiceId)> CreateCompletedOrderWithInvoiceAsync()
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

        return (createdOrder.Id, createInvoiceResult.Value!.Id);
    }
}
