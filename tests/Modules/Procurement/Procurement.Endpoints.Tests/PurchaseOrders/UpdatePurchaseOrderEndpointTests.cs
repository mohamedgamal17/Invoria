using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Invoria.BuildingBlocks.Infrastructure.Common;
using Invoria.Procurement.Contracts.Dtos;
using Invoria.Procurement.Endpoints.Parties.Requests;
using Invoria.Procurement.Endpoints.PurchaseOrders.Requests;

namespace Invoria.Procurement.Endpoints.Tests.PurchaseOrders;

[TestFixture]
public class UpdatePurchaseOrderEndpointTests : ProcurementTestFixture
{
    [Test]
    public async Task Should_update_purchase_order_when_it_is_draft()
    {
        var purchaseOrderId = await CreateDraftPurchaseOrderAsync();
        var supplierId = await CreateSupplierAsync();

        var request = new UpdatePurchaseOrderRequest
        {
            Id = purchaseOrderId,
            SupplierId = supplierId,
            TaxAmount = 1m,
            DiscountAmount = 0m,
            OrderDate = DateTime.UtcNow.Date,
            ExpectedDeliveryDate = DateTime.UtcNow.Date.AddDays(5),
            PurchaseOrderItems =
            [
                new PurchaseOrderItemRequest
                {
                    ProductId = Guid.NewGuid().ToString("N"),
                    Quantity = 2,
                    UnitPrice = 10m,
                    SupplierProductCode = "SKU-01"
                }
            ]
        };

        var response = await Client.PutAsJsonAsync($"/purchase-orders/{purchaseOrderId}", request);

        response.IsSuccessStatusCode.Should().BeTrue();
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PurchaseOrderDto>>();
        envelope.Should().NotBeNull();
        envelope!.Result.Should().NotBeNull();
        envelope.Result!.Id.Should().Be(purchaseOrderId);
        envelope.Result.SupplierId.Should().Be(supplierId);
        envelope.Result.SubTotal.Should().Be(20m);
        envelope.Result.TotalAmount.Should().Be(21m);
    }

    [Test]
    public async Task Should_update_purchase_order_when_it_is_reopened()
    {
        var purchaseOrderId = await CreateAndReopenPurchaseOrderAsync();
        var supplierId = await CreateSupplierAsync();

        var request = new UpdatePurchaseOrderRequest
        {
            Id = purchaseOrderId,
            SupplierId = supplierId,
            TaxAmount = 0m,
            DiscountAmount = 0m,
            OrderDate = DateTime.UtcNow.Date,
            ExpectedDeliveryDate = DateTime.UtcNow.Date.AddDays(1),
            PurchaseOrderItems =
            [
                new PurchaseOrderItemRequest
                {
                    ProductId = Guid.NewGuid().ToString("N"),
                    Quantity = 1,
                    UnitPrice = 5m,
                    SupplierProductCode = null
                }
            ]
        };

        var response = await Client.PutAsJsonAsync($"/purchase-orders/{purchaseOrderId}", request);

        response.IsSuccessStatusCode.Should().BeTrue();
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PurchaseOrderDto>>();
        envelope.Should().NotBeNull();
        envelope!.Result.Should().NotBeNull();
        envelope.Result!.Id.Should().Be(purchaseOrderId);
        envelope.Result.State.Should().Be(Contracts.PurchaseOrders.PurchaseState.Reopened);
        envelope.Result.SupplierId.Should().Be(supplierId);
        envelope.Result.TotalAmount.Should().Be(5m);
    }

    [Test]
    public async Task Should_return_404_when_purchase_order_is_not_exist()
    {
        var supplierId = await CreateSupplierAsync();
        var purchaseOrderId = Guid.NewGuid().ToString("N");

        var request = new UpdatePurchaseOrderRequest
        {
            Id = purchaseOrderId,
            SupplierId = supplierId,
            TaxAmount = 0m,
            DiscountAmount = 0m,
            OrderDate = null,
            ExpectedDeliveryDate = null,
            PurchaseOrderItems =
            [
                new PurchaseOrderItemRequest
                {
                    ProductId = Guid.NewGuid().ToString("N"),
                    Quantity = 1,
                    UnitPrice = 5m
                }
            ]
        };

        var response = await Client.PutAsJsonAsync($"/purchase-orders/{purchaseOrderId}", request);

        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task Should_return_domain_error_envelope_when_purchase_order_cannot_be_updated()
    {
        var purchaseOrderId = await CreateAndSubmitPurchaseOrderAsync();
        var supplierId = await CreateSupplierAsync();

        var request = new UpdatePurchaseOrderRequest
        {
            Id = purchaseOrderId,
            SupplierId = supplierId,
            TaxAmount = 0m,
            DiscountAmount = 0m,
            OrderDate = DateTime.UtcNow.Date,
            ExpectedDeliveryDate = DateTime.UtcNow.Date.AddDays(1),
            PurchaseOrderItems =
            [
                new PurchaseOrderItemRequest
                {
                    ProductId = Guid.NewGuid().ToString("N"),
                    Quantity = 1,
                    UnitPrice = 5m
                }
            ]
        };

        var response = await Client.PutAsJsonAsync($"/purchase-orders/{purchaseOrderId}", request);

        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope>();
        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeFalse();
        envelope.Error.Should().NotBeNull();
    }

    private async Task<string> CreateSupplierAsync()
    {
        var createSupplierRequest = new CreateSupplierRequest
        {
            SupplierCode = "SUP-" + Guid.NewGuid().ToString("N")[..8],
            Name = "Supplier for update purchase order",
            ContactEmail = "supplier@example.com",
            Phone = "+1"
        };
        var supplierResponse = await Client.PostAsJsonAsync("/suppliers", createSupplierRequest);
        supplierResponse.EnsureSuccessStatusCode();
        var supplierEnvelope = await supplierResponse.Content.ReadFromJsonAsync<Envelope<SupplierDto>>();
        return supplierEnvelope!.Result!.Id!;
    }

    private async Task<string> CreateDraftPurchaseOrderAsync()
    {
        var supplierId = await CreateSupplierAsync();

        var request = new CreatePurchaseOrderRequest
        {
            SupplierId = supplierId!,
            TaxAmount = 0m,
            DiscountAmount = 0m,
            OrderDate = DateTime.UtcNow.Date,
            ExpectedDeliveryDate = DateTime.UtcNow.Date.AddDays(7),
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

    private async Task<string> CreateAndSubmitPurchaseOrderAsync()
    {
        var purchaseOrderId = await CreateDraftPurchaseOrderAsync();

        var submitResponse = await Client.PostAsync(
            $"/purchase-orders/{purchaseOrderId}/submit",
            JsonContent.Create(new { }));
        submitResponse.EnsureSuccessStatusCode();

        return purchaseOrderId;
    }

    private async Task<string> CreateAndReopenPurchaseOrderAsync()
    {
        var purchaseOrderId = await CreateAndSubmitPurchaseOrderAsync();

        var approveResponse = await Client.PostAsync(
            $"/purchase-orders/{purchaseOrderId}/approve",
            JsonContent.Create(new { }));
        approveResponse.EnsureSuccessStatusCode();

        var reopenResponse = await Client.PostAsync(
            $"/purchase-orders/{purchaseOrderId}/reopen",
            JsonContent.Create(new { }));
        reopenResponse.EnsureSuccessStatusCode();

        return purchaseOrderId;
    }
}

