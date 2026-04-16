using FluentAssertions;
using Invoria.Application.Tests.Extensions;
using Invoria.Procurement.Application.Parties.Commands.CreateSupplier;
using Invoria.Procurement.Domain.Parties;
using Invoria.Procurement.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Procurement.Application.Tests.Parties;

[TestFixture]
public class CreateSupplierCommandHandlerTests : ProcurementTestFixture
{
    private IProcurementRepository<Supplier> SupplierRepository { get; }
    private IMediator Mediator { get; }

    public CreateSupplierCommandHandlerTests()
    {
        SupplierRepository = ServiceProvider.GetRequiredService<IProcurementRepository<Supplier>>();
        Mediator = ServiceProvider.GetRequiredService<IMediator>();
    }

    [Test]
    public async Task Should_create_supplier()
    {
        // Arrange
        var supplierCode = "SUP-" + Guid.NewGuid().ToString("N")[..8];
        var command = new CreateSupplierCommand(
            supplierCode: supplierCode,
            name: "Acme Corp",
            contactEmail: "acme@example.com",
            phone: "+1");

        // Act
        var result = await Mediator.Send(command);

        // Assert
        result.ShouldBeSuccess();

        var supplier = await SupplierRepository.SingleOrDefault(x => x.Id == result.Value!.Id);
        supplier.Should().NotBeNull();

        supplier!.SupplierCode.Should().Be(supplierCode);
        supplier.Name.Should().Be("Acme Corp");
        supplier.ContactEmail.Should().Be("acme@example.com");
        supplier.Phone.Should().Be("+1");

        result.Value!.SupplierCode.Should().Be(supplier.SupplierCode);
        result.Value.Name.Should().Be(supplier.Name);
        result.Value.ContactEmail.Should().Be(supplier.ContactEmail);
        result.Value.Phone.Should().Be(supplier.Phone);
    }
}

