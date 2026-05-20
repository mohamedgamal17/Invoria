using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Invoria.BuildingBlocks.Infrastructure.Common;
using Invoria.Procurement.Contracts.Dtos;
using Invoria.Procurement.Endpoints.Parties.Requests;
using Invoria.Procurement.Endpoints.PurchaseOrders.Requests;

namespace Invoria.Procurement.Endpoints.Tests.PurchaseOrders;

[TestFixture]
public class CreatePurchaseOrderEndpointTests : ProcurementTestFixture
{
    [Test]
    public async Task Should_create_purchase_order()
    {
        var createSupplierRequest = new CreateSupplierRequest
        {
            SupplierCode = "SUP-" + Guid.NewGuid().ToString("N")[..8],
            Name = "Supplier for purchase order",
            ContactEmail = "supplier@example.com",
            Phone = "+1"
        };
        var supplierResponse = await Client.PostAsJsonAsync("/suppliers", createSupplierRequest);
        supplierResponse.EnsureSuccessStatusCode();
        var supplierEnvelope = await supplierResponse.Content.ReadFromJsonAsync<Envelope<SupplierDto>>();
        var supplierId = supplierEnvelope!.Result!.Id;

        var request = new CreatePurchaseOrderRequest
        {
            SupplierId = supplierId!,
            TaxAmount = 10m,
            DiscountAmount = 5m,
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

        response.IsSuccessStatusCode.Should().BeTrue();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task Should_return_validation_errors_envelope_when_request_is_invalid()
    {
        var request = new CreatePurchaseOrderRequest
        {
            SupplierId = "",
            TaxAmount = -1m,
            DiscountAmount = -1m,
            PurchaseOrderItems = []
        };

        var response = await Client.PostAsJsonAsync("/purchase-orders", request);

        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope>();
        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeFalse();
        envelope.Error.Should().NotBeNull();
        envelope.Error!.Status.Should().Be((int)HttpStatusCode.BadRequest);
        envelope.Error.Errors.Should().NotBeEmpty();
    }
}
