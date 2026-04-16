using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Invoria.BuildingBlocks.Infrastructure.Common;
using Invoria.Procurement.Contracts.Dtos;
using Invoria.Procurement.Domain.Parties;
using Invoria.Procurement.Domain.PurchaseOrders;
using Invoria.Procurement.Domain.Repositories;
using Invoria.Procurement.Endpoints.Parties.Requests;
using Invoria.Procurement.Endpoints.PurchaseOrders.Requests;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Procurement.Endpoints.Tests.PurchaseOrders;

[TestFixture]
public class SubmitPurchaseOrderEndpointTests : ProcurementTestFixture
{
    [Test]
    public async Task Should_submit_purchase_order_when_it_is_draft()
    {
        var purchaseOrderId = await CreatePurchaseOrderAsync();

        var response = await Client.PostAsync(
            $"/purchase-orders/{purchaseOrderId}/submit",
            new StringContent("{}", Encoding.UTF8, "application/json"));

        response.IsSuccessStatusCode.Should().BeTrue();
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PurchaseOrderDto>>();
        envelope.Should().NotBeNull();
        envelope!.Result.Should().NotBeNull();
    }

    [Test]
    public async Task Should_return_domain_error_envelope_when_purchase_order_cannot_be_submitted()
    {
        var purchaseOrderId = await CreatePurchaseOrderAsync();

        var firstSubmit = await Client.PostAsync(
            $"/purchase-orders/{purchaseOrderId}/submit",
            new StringContent("{}", Encoding.UTF8, "application/json"));
        firstSubmit.EnsureSuccessStatusCode();

        var secondSubmit = await Client.PostAsync(
            $"/purchase-orders/{purchaseOrderId}/submit",
            new StringContent("{}", Encoding.UTF8, "application/json"));

        secondSubmit.IsSuccessStatusCode.Should().BeFalse();
        secondSubmit.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        var envelope = await secondSubmit.Content.ReadFromJsonAsync<Envelope>();
        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeFalse();
        envelope.Error.Should().NotBeNull();
    }

    [Test]
    public async Task Should_return_domain_error_envelope_when_purchase_order_has_no_items()
    {
        var purchaseOrderId = await CreateEmptyDraftPurchaseOrderAsync();

        var response = await Client.PostAsync(
            $"/purchase-orders/{purchaseOrderId}/submit",
            new StringContent("{}", Encoding.UTF8, "application/json"));

        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope>();
        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeFalse();
        envelope.Error.Should().NotBeNull();
    }

    private async Task<string> CreatePurchaseOrderAsync()
    {
        var createSupplierRequest = new CreateSupplierRequest
        {
            SupplierCode = "SUP-" + Guid.NewGuid().ToString("N")[..8],
            Name = "Supplier for submit purchase order",
            ContactEmail = "submit-supplier@example.com",
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

        var response = await Client.PostAsJsonAsync("/purchase-orders", request);
        response.EnsureSuccessStatusCode();
        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PurchaseOrderDto>>();
        return envelope!.Result!.Id!;
    }

    private async Task<string> CreateEmptyDraftPurchaseOrderAsync()
    {
        var supplierRepository = Scope.ServiceProvider.GetRequiredService<IProcurementRepository<Supplier>>();
        var purchaseOrderRepository = Scope.ServiceProvider.GetRequiredService<IProcurementRepository<PurchaseOrder>>();

        var supplier = Supplier.Create(
            id: Guid.NewGuid().ToString("N"),
            supplierCode: "SUP-" + Guid.NewGuid().ToString("N")[..8],
            name: "Supplier for empty order",
            contactEmail: "empty-order@example.com",
            phone: "+1",
            createdBy: null);
        await supplierRepository.Add(supplier);

        var purchaseOrder = new PurchaseOrder(
            id: Guid.NewGuid().ToString("N"),
            purchaseNumber: "PO-" + Guid.NewGuid().ToString("N")[..8],
            supplierId: supplier.Id,
            orderDate: DateTime.UtcNow.Date,
            expectedDeliveryDate: DateTime.UtcNow.Date.AddDays(7),
            createdBy: null);
        await purchaseOrderRepository.Add(purchaseOrder);

        return purchaseOrder.Id;
    }
}
