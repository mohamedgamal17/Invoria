using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Infrastructure.Common;
using Invoria.Endpoints.Tests.Utilities;
using Invoria.Procurement.Contracts.Dtos;
using Invoria.Procurement.Endpoints.Parties.Requests;
using Invoria.Procurement.Endpoints.PurchaseOrders.Requests;

namespace Invoria.Procurement.Endpoints.Tests.PurchaseOrders;

[TestFixture]
public class ListPurchaseOrdersEndpointTests : ProcurementTestFixture
{
    [Test]
    public async Task Should_return_paged_list_including_created_purchase_order()
    {
        var created = await CreatePurchaseOrderAsync("SRCH");
        var filter = created.PurchaseNumber[3..];

        var query = new { Skip = 0, Length = 100, Number = filter };
        var uri = "/purchase-orders?" + QueryStringHelper.ToQueryString(query);

        var response = await Client.GetAsync(uri);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PagingDto<PurchaseOrderDto>>>();
        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeTrue();
        envelope.Result.Should().NotBeNull();
        envelope.Result!.Data.Should().Contain(x => x.Id == created.Id);
    }

    [Test]
    public async Task Should_filter_by_number_and_ignore_whitespace_number()
    {
        var matching = await CreatePurchaseOrderAsync("FLTR");
        await CreatePurchaseOrderAsync("PO-OTHER-" + Guid.NewGuid().ToString("N")[..6]);

        var filter = matching.PurchaseNumber.ToLower();
        var query = new { Skip = 0, Length = 100, Number = $" {filter.ToLower()} " };
        var uri = "/purchase-orders?" + QueryStringHelper.ToQueryString(query);

        var response = await Client.GetAsync(uri);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PagingDto<PurchaseOrderDto>>>();
        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeTrue();
        envelope.Result.Should().NotBeNull();
        envelope.Result!.Data.Should().ContainSingle(x => x.Id == matching.Id);
    }

    [Test]
    public async Task Should_return_validation_errors_envelope_when_request_is_invalid()
    {
        var query = new { Skip = 0, Length = 0 };
        var uri = "/purchase-orders?" + QueryStringHelper.ToQueryString(query);

        var response = await Client.GetAsync(uri);

        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope>();
        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeFalse();
        envelope.Error.Should().NotBeNull();
        envelope.Error!.Status.Should().Be((int)HttpStatusCode.BadRequest);
        envelope.Error.Errors.Should().NotBeEmpty();
    }

    [Test]
    public async Task Should_use_default_false_include_flags_when_indicators_are_omitted()
    {
        var created = await CreatePurchaseOrderAsync("NOINC");
        var query = new { Skip = 0, Length = 100, Number = created.PurchaseNumber };
        var uri = "/purchase-orders?" + QueryStringHelper.ToQueryString(query);

        var response = await Client.GetAsync(uri);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PagingDto<PurchaseOrderDto>>>();
        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeTrue();
        envelope.Result.Should().NotBeNull();
        envelope.Result!.Data.Should().ContainSingle(x => x.Id == created.Id);
    }

    [Test]
    public async Task Should_include_purchase_items_when_indicator_is_true()
    {
        var created = await CreatePurchaseOrderAsync("INCITEMS");
        var query = new
        {
            Skip = 0,
            Length = 100,
            Number = created.PurchaseNumber,
            IncludePurchaseItems = true
        };
        var uri = "/purchase-orders?" + QueryStringHelper.ToQueryString(query);

        var response = await Client.GetAsync(uri);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PagingDto<PurchaseOrderDto>>>();
        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeTrue();
        envelope.Result.Should().NotBeNull();
        var dto = envelope.Result!.Data.Single(x => x.Id == created.Id);
        dto.PurchaseOrderItems.Should().ContainSingle();
    }

    [Test]
    public async Task Should_accept_include_supplier_when_indicator_is_true()
    {
        var created = await CreatePurchaseOrderAsync("INCSUP");
        var query = new
        {
            Skip = 0,
            Length = 100,
            Number = created.PurchaseNumber,
            IncludeSupplier = true
        };
        var uri = "/purchase-orders?" + QueryStringHelper.ToQueryString(query);

        var response = await Client.GetAsync(uri);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PagingDto<PurchaseOrderDto>>>();
        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeTrue();
        envelope.Result.Should().NotBeNull();
        envelope.Result!.Data.Should().ContainSingle(x => x.Id == created.Id);
    }

    private async Task<PurchaseOrderDto> CreatePurchaseOrderAsync(string supplierProductCode)
    {
        var createSupplierRequest = new CreateSupplierRequest
        {
            SupplierCode = "SUP-" + Guid.NewGuid().ToString("N")[..8],
            Name = "Supplier for list purchase orders",
            ContactEmail = "list-po@example.com",
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
            OrderDate = DateTime.UtcNow.Date,
            ExpectedDeliveryDate = DateTime.UtcNow.Date.AddDays(7),
            PurchaseOrderItems =
            [
                new PurchaseOrderItemRequest
                {
                    ProductId = Guid.NewGuid().ToString("N"),
                    Quantity = 1,
                    UnitPrice = 100m,
                    SupplierProductCode = supplierProductCode
                }
            ]
        };

        var response = await Client.PostAsJsonAsync("/purchase-orders", createRequest);
        response.EnsureSuccessStatusCode();
        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PurchaseOrderDto>>();
        envelope.Should().NotBeNull();
        envelope!.Result.Should().NotBeNull();
        envelope.Result!.PurchaseNumber.Should().NotBeNullOrWhiteSpace();
        return envelope.Result;
    }
}
