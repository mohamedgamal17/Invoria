using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Invoria.Procurement.Domain.Parties;
using Invoria.Procurement.Domain.Repositories;
using Invoria.Procurement.Endpoints.Parties.Requests;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Procurement.Endpoints.Tests.Suppliers;

[TestFixture]
public class UpdateSupplierEndpointTests : ProcurementTestFixture
{
    private IProcurementRepository<Supplier> SupplierRepository { get; set; } = null!;

    public UpdateSupplierEndpointTests()
    {
        SupplierRepository = Scope.ServiceProvider.GetRequiredService<IProcurementRepository<Supplier>>();
    }

    [Test]
    public async Task Should_update_supplier()
    {
        var supplier = await CreateSupplierAsync();

        var request = new UpdateSupplierRequest
        {
            Id = supplier.Id,
            SupplierCode = "SUP-" + Guid.NewGuid().ToString("N")[..8],
            Name = "Updated Name",
            ContactEmail = "updated@example.com",
            Phone = "+2"
        };

        var response = await Client.PutAsJsonAsync("/suppliers/" + supplier.Id, request);

        response.IsSuccessStatusCode.Should().BeTrue();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Test]
    public async Task Should_return_failure_status_code_404_when_supplier_is_not_exist()
    {
        var request = new UpdateSupplierRequest
        {
            Id = Guid.NewGuid().ToString("N"),
            SupplierCode = "SUP-" + Guid.NewGuid().ToString("N")[..8],
            Name = "Updated Name",
            ContactEmail = null,
            Phone = null
        };

        var response = await Client.PutAsJsonAsync("/suppliers/" + request.Id, request);

        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<Supplier> CreateSupplierAsync()
    {
        var supplier = Supplier.Create(
            id: Guid.NewGuid().ToString("N"),
            supplierCode: "SUP-" + Guid.NewGuid().ToString("N")[..8],
            name: "Acme Corp",
            contactEmail: null,
            phone: null,
            createdBy: "tests");

        return await SupplierRepository.Add(supplier);
    }
}

