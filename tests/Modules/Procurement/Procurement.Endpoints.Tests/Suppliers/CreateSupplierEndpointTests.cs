using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Invoria.BuildingBlocks.Infrastructure.Common;
using Invoria.Procurement.Endpoints.Parties.Requests;

namespace Invoria.Procurement.Endpoints.Tests.Suppliers;

[TestFixture]
public class CreateSupplierEndpointTests : ProcurementTestFixture
{
    [Test]
    public async Task Should_create_supplier()
    {
        var request = new CreateSupplierRequest
        {
            SupplierCode = "SUP-" + Guid.NewGuid().ToString("N")[..8],
            Name = "Acme Corp",
            ContactEmail = "acme@example.com",
            Phone = "+1"
        };

        var response = await Client.PostAsJsonAsync("/suppliers", request);

        response.IsSuccessStatusCode.Should().BeTrue();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task Should_return_validation_errors_envelope_when_request_is_invalid()
    {
        var request = new CreateSupplierRequest
        {
            SupplierCode = "",
            Name = "a",
            ContactEmail = new string('a', 300),
            Phone = new string('1', 200)
        };

        var response = await Client.PostAsJsonAsync("/suppliers", request);

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

