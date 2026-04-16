using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.Procurement.Application.Parties.Queries.GetSupplierById;
using Invoria.Procurement.Domain.Parties;
using Invoria.Procurement.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Procurement.Application.Tests.Parties;

[TestFixture]
public class GetSupplierByIdQueryHandlerTests : ProcurementTestFixture
{
    private IProcurementRepository<Supplier> SupplierRepository { get; }
    private IMediator Mediator { get; }

    public GetSupplierByIdQueryHandlerTests()
    {
        SupplierRepository = ServiceProvider.GetRequiredService<IProcurementRepository<Supplier>>();
        Mediator = ServiceProvider.GetRequiredService<IMediator>();
    }

    [Test]
    public async Task Should_return_supplier_when_found()
    {
        var supplier = await CreateSupplierAsync(
            supplierCode: "SUP-" + Guid.NewGuid().ToString("N")[..8],
            name: "Acme Corp",
            contactEmail: "acme@example.com",
            phone: "+1");

        var query = new GetSupplierByIdQuery { Id = supplier.Id };

        var result = await Mediator.Send(query);

        result.ShouldBeSuccess();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(supplier.Id);
        result.Value.SupplierCode.Should().Be(supplier.SupplierCode);
        result.Value.Name.Should().Be(supplier.Name);
        result.Value.ContactEmail.Should().Be(supplier.ContactEmail);
        result.Value.Phone.Should().Be(supplier.Phone);
    }

    [Test]
    public async Task Should_return_failure_when_supplier_not_found()
    {
        var query = new GetSupplierByIdQuery { Id = Guid.NewGuid().ToString("N") };

        var result = await Mediator.Send(query);

        result.IsSuccess.Should().BeFalse();
        result.Exception.Should().NotBeNull();
        result.Exception.Should().BeOfType<NotFoundException>();
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
