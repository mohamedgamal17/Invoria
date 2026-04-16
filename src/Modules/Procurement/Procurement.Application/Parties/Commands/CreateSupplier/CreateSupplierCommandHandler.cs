using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Procurement.Application.Parties.Factories;
using Invoria.Procurement.Contracts.Dtos;
using Invoria.Procurement.Domain.Parties;
using Invoria.Procurement.Domain.Repositories;

namespace Invoria.Procurement.Application.Parties.Commands.CreateSupplier;

public sealed class CreateSupplierCommandHandler : IApplicatonRequestHandler<CreateSupplierCommand, SupplierDto>
{
    private readonly IProcurementRepository<Supplier> _supplierRepository;
    private readonly ISupplierResponseFactory _supplierResponseFactory;

    public CreateSupplierCommandHandler(
        IProcurementRepository<Supplier> supplierRepository,
        ISupplierResponseFactory supplierResponseFactory)
    {
        _supplierRepository = supplierRepository;
        _supplierResponseFactory = supplierResponseFactory;
    }

    public async Task<Result<SupplierDto>> Handle(CreateSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = Supplier.Create(
            id: Guid.NewGuid().ToString("N"),
            supplierCode: request.SupplierCode,
            name: request.Name,
            contactEmail: request.ContactEmail,
            phone: request.Phone,
            createdBy: null);

        await _supplierRepository.Add(supplier, cancellationToken);

        var dto = await _supplierResponseFactory.PrepareDto(supplier);

        return dto;
    }
}

