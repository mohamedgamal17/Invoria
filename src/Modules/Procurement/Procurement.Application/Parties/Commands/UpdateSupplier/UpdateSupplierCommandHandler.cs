using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Procurement.Application.Parties.Factories;
using Invoria.Procurement.Contracts.Dtos;
using Invoria.Procurement.Domain.Parties;
using Invoria.Procurement.Domain.Repositories;

namespace Invoria.Procurement.Application.Parties.Commands.UpdateSupplier;

public sealed class UpdateSupplierCommandHandler : IApplicatonRequestHandler<UpdateSupplierCommand, SupplierDto>
{
    private readonly IProcurementRepository<Supplier> _supplierRepository;
    private readonly ISupplierResponseFactory _supplierResponseFactory;

    public UpdateSupplierCommandHandler(
        IProcurementRepository<Supplier> supplierRepository,
        ISupplierResponseFactory supplierResponseFactory)
    {
        _supplierRepository = supplierRepository;
        _supplierResponseFactory = supplierResponseFactory;
    }

    public async Task<Result<SupplierDto>> Handle(UpdateSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _supplierRepository.SingleOrDefault(x => x.Id == request.Id, cancellationToken);

        if (supplier == null)
        {
            return Result.Failure<SupplierDto>(new NotFoundException($"Supplier with ID {request.Id} not found"));
        }

        supplier.Update(
            supplierCode: request.SupplierCode,
            name: request.Name,
            contactEmail: request.ContactEmail,
            phone: request.Phone,
            lastModifiedBy: null);

        await _supplierRepository.Update(supplier, cancellationToken);

        var dto = await _supplierResponseFactory.PrepareDto(supplier);

        return dto;
    }
}

