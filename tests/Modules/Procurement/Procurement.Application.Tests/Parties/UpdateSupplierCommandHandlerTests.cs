using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.Procurement.Application.Parties.Commands.UpdateSupplier;
using Invoria.Procurement.Domain.Parties;
using Invoria.Procurement.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Procurement.Application.Tests.Parties;

[TestFixture]
public class UpdateSupplierCommandHandlerTests : ProcurementTestFixture
{
    private IProcurementRepository<Supplier> SupplierRepository { get; }
    private IMediator Mediator { get; }

    public UpdateSupplierCommandHandlerTests()
    {
        SupplierRepository = ServiceProvider.GetRequiredService<IProcurementRepository<Supplier>>();
        Mediator = ServiceProvider.GetRequiredService<IMediator>();
    }

    [Test]
    public async Task Should_update_supplier()
    {
        // Arrange
        var oldSupplierCode = "SUP-" + Guid.NewGuid().ToString("N")[..8];
        var newSupplierCode = "SUP-" + Guid.NewGuid().ToString("N")[..8];
        var existing = await CreateSupplierAsync(
            supplierCode: oldSupplierCode,
            name: "Old Name",
            contactEmail: null,
            phone: null);

        var command = new UpdateSupplierCommand(
            id: existing.Id,
            supplierCode: newSupplierCode,
            name: "New Name",
            contactEmail: "new@example.com",
            phone: "+2");

        // Act
        var result = await Mediator.Send(command);

        // Assert
        result.ShouldBeSuccess();

        var supplier = await SupplierRepository.SingleOrDefault(x => x.Id == existing.Id);
        supplier.Should().NotBeNull();
        supplier!.SupplierCode.Should().Be(newSupplierCode);
        supplier.Name.Should().Be("New Name");
        supplier.ContactEmail.Should().Be("new@example.com");
        supplier.Phone.Should().Be("+2");

        result.Value!.Id.Should().Be(existing.Id);
        result.Value.SupplierCode.Should().Be(newSupplierCode);
        result.Value.Name.Should().Be("New Name");
    }

    [Test]
    public async Task Should_return_failure_when_supplier_not_found()
    {
        // Arrange
        var command = new UpdateSupplierCommand(
            id: Guid.NewGuid().ToString("N"),
            supplierCode: "SUP-NEW",
            name: "New Name",
            contactEmail: null,
            phone: null);

        // Act
        var result = await Mediator.Send(command);

        // Assert
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

