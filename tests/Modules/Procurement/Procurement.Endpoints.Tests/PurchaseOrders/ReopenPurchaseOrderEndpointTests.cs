using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Invoria.BuildingBlocks.Infrastructure.Common;
using Invoria.Procurement.Contracts.Dtos;
using Invoria.Procurement.Endpoints.Parties.Requests;
using Invoria.Procurement.Endpoints.PurchaseOrders.Requests;

namespace Invoria.Procurement.Endpoints.Tests.PurchaseOrders;

[TestFixture]
public class ReopenPurchaseOrderEndpointTests : ProcurementTestFixture
{
    [Test]
    public async Task Should_reopen_purchase_order_when_it_is_submitted()
    {
        var purchaseOrderId = await CreateAndSubmitPurchaseOrderAsync();

        var response = await Client.PostAsync(
            $"/purchase-orders/{purchaseOrderId}/reopen",
            new StringContent("{}", Encoding.UTF8, "application/json"));

        response.IsSuccessStatusCode.Should().BeTrue();
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PurchaseOrderDto>>();
        envelope.Should().NotBeNull();
        envelope!.Result.Should().NotBeNull();
        envelope.Result!.State.Should().Be(Contracts.PurchaseOrders.PurchaseState.Reopened);
    }

    [Test]
    public async Task Should_reopen_purchase_order_when_it_is_approved()
    {
        var purchaseOrderId = await CreateApprovePurchaseOrderAsync();

        var response = await Client.PostAsync(
            $"/purchase-orders/{purchaseOrderId}/reopen",
            new StringContent("{}", Encoding.UTF8, "application/json"));

        response.IsSuccessStatusCode.Should().BeTrue();
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PurchaseOrderDto>>();
        envelope.Should().NotBeNull();
        envelope!.Result.Should().NotBeNull();
        envelope.Result!.State.Should().Be(Contracts.PurchaseOrders.PurchaseState.Reopened);
    }

    [Test]
    public async Task Should_return_domain_error_envelope_when_purchase_order_cannot_be_reopened()
    {
        var purchaseOrderId = await CreateDraftPurchaseOrderAsync();

        var response = await Client.PostAsync(
            $"/purchase-orders/{purchaseOrderId}/reopen",
            new StringContent("{}", Encoding.UTF8, "application/json"));

        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope>();
        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeFalse();
        envelope.Error.Should().NotBeNull();
    }

    private async Task<string> CreateApprovePurchaseOrderAsync()
    {
        var purchaseOrderId = await CreateAndSubmitPurchaseOrderAsync();

        var approveResponse = await Client.PostAsync(
            $"/purchase-orders/{purchaseOrderId}/approve",
            new StringContent("{}", Encoding.UTF8, "application/json"));
        approveResponse.EnsureSuccessStatusCode();

        return purchaseOrderId;
    }

    private async Task<string> CreateAndSubmitPurchaseOrderAsync()
    {
        var purchaseOrderId = await CreateDraftPurchaseOrderAsync();

        var submitResponse = await Client.PostAsync(
            $"/purchase-orders/{purchaseOrderId}/submit",
            new StringContent("{}", Encoding.UTF8, "application/json"));
        submitResponse.EnsureSuccessStatusCode();

        return purchaseOrderId;
    }

    private async Task<string> CreateDraftPurchaseOrderAsync()
    {
        var createSupplierRequest = new CreateSupplierRequest
        {
            SupplierCode = "SUP-" + Guid.NewGuid().ToString("N")[..8],
            Name = "Draft supplier for reopen purchase order",
            ContactEmail = "reopen-draft-supplier@example.com",
            Phone = "+1"
        };
        var supplierResponse = await Client.PostAsJsonAsync("/suppliers", createSupplierRequest);
        supplierResponse.EnsureSuccessStatusCode();
        var supplierEnvelope = await supplierResponse.Content.ReadFromJsonAsync<Envelope<SupplierDto>>();
        var supplierId = supplierEnvelope!.Result!.Id;

        var request = new CreatePurchaseOrderRequest
        {
            SupplierId = supplierId!,
            TaxAmount = 0m,
            DiscountAmount = 0m,
            PurchaseOrderItems =
            [
                new PurchaseOrderItemRequest
                {
                    ProductId = Guid.NewGuid().ToString("N"),
                    Quantity = 2,
                    UnitPrice = 100m,
                    SupplierProductCode = "SKU-01"
                }
            ]
        };

        var createResponse = await Client.PostAsJsonAsync("/purchase-orders", request);
        createResponse.EnsureSuccessStatusCode();
        var createEnvelope = await createResponse.Content.ReadFromJsonAsync<Envelope<PurchaseOrderDto>>();
        return createEnvelope!.Result!.Id!;
    }
}
