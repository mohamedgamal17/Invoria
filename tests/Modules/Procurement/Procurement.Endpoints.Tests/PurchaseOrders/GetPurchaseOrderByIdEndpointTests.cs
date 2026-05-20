using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Invoria.BuildingBlocks.Infrastructure.Common;
using Invoria.Procurement.Contracts.Dtos;
using Invoria.Procurement.Endpoints.Parties.Requests;
using Invoria.Procurement.Endpoints.PurchaseOrders.Requests;

namespace Invoria.Procurement.Endpoints.Tests.PurchaseOrders;

[TestFixture]
public class GetPurchaseOrderByIdEndpointTests : ProcurementTestFixture
{
    [Test]
    public async Task Should_return_purchase_order_when_found()
    {
        var purchaseOrder = await CreatePurchaseOrderAsync();

        var response = await Client.GetAsync("/purchase-orders/" + purchaseOrder.Id);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PurchaseOrderDto>>();
        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeTrue();
        envelope.Result.Should().NotBeNull();
        envelope.Result!.Id.Should().Be(purchaseOrder.Id);
        envelope.Result.PurchaseNumber.Should().Be(purchaseOrder.PurchaseNumber);
        envelope.Result.SupplierId.Should().Be(purchaseOrder.SupplierId);
    }

    [Test]
    public async Task Should_return_404_when_purchase_order_not_found()
    {
        var response = await Client.GetAsync("/purchase-orders/" + Guid.NewGuid().ToString("N"));

        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<PurchaseOrderDto> CreatePurchaseOrderAsync()
    {
        var createSupplierRequest = new CreateSupplierRequest
        {
            SupplierCode = "SUP-" + Guid.NewGuid().ToString("N")[..8],
            Name = "Supplier for get purchase order",
            ContactEmail = "get-po@example.com",
            Phone = "+1"
        };

        var supplierResponse = await Client.PostAsJsonAsync("/suppliers", createSupplierRequest);
        supplierResponse.EnsureSuccessStatusCode();
        var supplierEnvelope = await supplierResponse.Content.ReadFromJsonAsync<Envelope<SupplierDto>>();
        var supplierId = supplierEnvelope!.Result!.Id;

        var createRequest = new CreatePurchaseOrderRequest
        {
            SupplierId = supplierId!,
            TaxAmount = 0m,
            DiscountAmount = 0m,
            PurchaseOrderItems =
            [
                new PurchaseOrderItemRequest
                {
                    ProductId = Guid.NewGuid().ToString("N"),
                    Quantity = 1,
                    UnitPrice = 100m,
                    SupplierProductCode = "SKU-GET-01"
                }
            ]
        };

        var response = await Client.PostAsJsonAsync("/purchase-orders", createRequest);
        response.EnsureSuccessStatusCode();
        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PurchaseOrderDto>>();
        envelope.Should().NotBeNull();
        envelope!.Result.Should().NotBeNull();
        return envelope.Result!;
    }
}
