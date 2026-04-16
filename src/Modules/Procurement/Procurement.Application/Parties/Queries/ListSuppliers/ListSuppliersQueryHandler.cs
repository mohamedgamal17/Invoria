using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.BuildingBlocks.Domain.Primitives;
using Invoria.BuildingBlocks.EntityFramework.Extensions;
using Invoria.Procurement.Application.Parties.Factories;
using Invoria.Procurement.Contracts.Dtos;
using Invoria.Procurement.Domain.Parties;
using Invoria.Procurement.Domain.Repositories;

namespace Invoria.Procurement.Application.Parties.Queries.ListSuppliers;

public sealed class ListSuppliersQueryHandler : IApplicatonRequestHandler<ListSupplierQuery, PagingDto<SupplierDto>>
{
    private readonly IProcurementRepository<Supplier> _supplierRepository;
    private readonly ISupplierResponseFactory _supplierResponseFactory;

    public ListSuppliersQueryHandler(
        IProcurementRepository<Supplier> supplierRepository,
        ISupplierResponseFactory supplierResponseFactory)
    {
        _supplierRepository = supplierRepository;
        _supplierResponseFactory = supplierResponseFactory;
    }

    public async Task<Result<PagingDto<SupplierDto>>> Handle(ListSupplierQuery request, CancellationToken cancellationToken)
    {
        var query = _supplierRepository.AsQuerable();

        var nameTerm = request.Name?.Trim();
        if (!string.IsNullOrEmpty(nameTerm))
        {
            var normalizedNameTerm = nameTerm.ToLower();
            query = query.Where(x => x.Name.ToLower().Contains(normalizedNameTerm));
        }

        var codeTerm = request.Code?.Trim();
        if (!string.IsNullOrEmpty(codeTerm))
        {
            var normalizedCodeTerm = codeTerm.ToLower();
            query = query.Where(x => x.SupplierCode.ToLower().Contains(normalizedCodeTerm));
        }

        query = query.OrderBy(x => x.Name).ThenBy(x => x.Id);

        var paged = await query.ToPaged(request.Skip, request.Length);
        var response = await _supplierResponseFactory.PreparePagingDto(paged);

        return response;
    }
}
