using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Invoria.BuildingBlocks.Infrastructure.Common;
using Invoria.Procurement.Contracts.Dtos;
using Invoria.Procurement.Domain.Parties;
using Invoria.Procurement.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Procurement.Endpoints.Tests.Suppliers;

[TestFixture]
public class GetSupplierByIdEndpointTests : ProcurementTestFixture
{
    private IProcurementRepository<Supplier> SupplierRepository { get; }

    public GetSupplierByIdEndpointTests()
    {
        SupplierRepository = Scope.ServiceProvider.GetRequiredService<IProcurementRepository<Supplier>>();
    }

    [Test]
    public async Task Should_return_supplier_when_found()
    {
        var supplier = await CreateSupplierAsync(
            supplierCode: "SUP-" + Guid.NewGuid().ToString("N")[..8],
            name: "Acme Corp",
            contactEmail: "acme@example.com",
            phone: "+1");

        var response = await Client.GetAsync("/suppliers/" + supplier.Id);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<Envelope<SupplierDto>>();
        envelope.Should().NotBeNull();
        envelope!.IsSuccess.Should().BeTrue();
        envelope.Result.Should().NotBeNull();
        envelope.Result!.Id.Should().Be(supplier.Id);
        envelope.Result.SupplierCode.Should().Be(supplier.SupplierCode);
        envelope.Result.Name.Should().Be(supplier.Name);
    }

    [Test]
    public async Task Should_return_404_when_supplier_not_found()
    {
        var response = await Client.GetAsync("/suppliers/" + Guid.NewGuid().ToString("N"));

        response.IsSuccessStatusCode.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<Supplier> CreateSupplierAsync(
        string supplierCode,
        string name,
        string? contactEmail,
        string? phone)
    {
        var supplier = Supplier.Create(
            id: Guid.NewGuid().ToString("N"),
            supplierCode: supplierCode,
            name: name,
            contactEmail: contactEmail,
            phone: phone,
            createdBy: "tests");

        return await SupplierRepository.Add(supplier);
    }
}
