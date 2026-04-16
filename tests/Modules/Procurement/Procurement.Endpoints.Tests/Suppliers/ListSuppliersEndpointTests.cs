using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Infrastructure.Common;
using Invoria.Endpoints.Tests.Utilities;
using Invoria.Procurement.Contracts.Dtos;

namespace Invoria.Procurement.Endpoints.Tests.Suppliers;

[TestFixture]
public class ListSuppliersEndpointTests : ProcurementTestFixture
{
    [Test]
    public async Task Should_return_paged_list_including_created_supplier()
    {
        var createRequest = new
        {
            SupplierCode = "SUP-" + Guid.NewGuid().ToString("N")[..8],
            Name = "Acme Searchable",
            ContactEmail = (string?)null,
            Phone = (string?)null
        };

        var createResponse = await Client.PostAsJsonAsync("/suppliers", createRequest);
        createResponse.IsSuccessStatusCode.Should().BeTrue();

        var query = new { Skip = 0, Length = 100, Name = "search" };
        var uri = "/suppliers?" + QueryStringHelper.ToQueryString(query);

        var response = await Client.GetAsync(uri);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PagingDto<SupplierDto>>>();
        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeTrue();
        envelope.Result.Should().NotBeNull();
        envelope.Result!.Data.Should().Contain(x => x.Name == createRequest.Name);
    }

    [Test]
    public async Task Should_filter_by_code_and_ignore_whitespace_name()
    {
        var matchingCode = "SUP-FLTR-" + Guid.NewGuid().ToString("N")[..6];
        var createMatching = await Client.PostAsJsonAsync("/suppliers", new
        {
            SupplierCode = matchingCode,
            Name = "Northwind",
            ContactEmail = (string?)null,
            Phone = (string?)null
        });
        createMatching.EnsureSuccessStatusCode();

        var createOther = await Client.PostAsJsonAsync("/suppliers", new
        {
            SupplierCode = "SUP-OTHER-" + Guid.NewGuid().ToString("N")[..6],
            Name = "Other Supplier",
            ContactEmail = (string?)null,
            Phone = (string?)null
        });
        createOther.EnsureSuccessStatusCode();

        var query = new { Skip = 0, Length = 100, Name = "   ", Code = "fltr" };
        var uri = "/suppliers?" + QueryStringHelper.ToQueryString(query);

        var response = await Client.GetAsync(uri);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<PagingDto<SupplierDto>>>();
        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeTrue();
        envelope.Result.Should().NotBeNull();
        envelope.Result!.Data.Should().ContainSingle(x => x.SupplierCode == matchingCode);
    }

    [Test]
    public async Task Should_return_validation_errors_envelope_when_request_is_invalid()
    {
        var query = new { Skip = 0, Length = 0 };
        var uri = "/suppliers?" + QueryStringHelper.ToQueryString(query);

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
}
