using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Exceptions;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.Procurement.Application.Parties.Factories;
using Invoria.Procurement.Contracts.Dtos;
using Invoria.Procurement.Domain.Parties;
using Invoria.Procurement.Domain.Repositories;

namespace Invoria.Procurement.Application.Parties.Queries.GetSupplierById;

public sealed class GetSupplierByIdQueryHandler : IApplicatonRequestHandler<GetSupplierByIdQuery, SupplierDto>
{
    private readonly IProcurementRepository<Supplier> _supplierRepository;
    private readonly ISupplierResponseFactory _supplierResponseFactory;

    public GetSupplierByIdQueryHandler(
        IProcurementRepository<Supplier> supplierRepository,
        ISupplierResponseFactory supplierResponseFactory)
    {
        _supplierRepository = supplierRepository;
        _supplierResponseFactory = supplierResponseFactory;
    }

    public async Task<Result<SupplierDto>> Handle(GetSupplierByIdQuery request, CancellationToken cancellationToken)
    {
        var supplier = await _supplierRepository.SingleOrDefault(x => x.Id == request.Id, cancellationToken);
        if (supplier is null)
        {
            return Result.Failure<SupplierDto>(new NotFoundException($"Supplier with ID {request.Id} not found"));
        }

        var dto = await _supplierResponseFactory.PrepareDto(supplier);
        return dto;
    }
}
