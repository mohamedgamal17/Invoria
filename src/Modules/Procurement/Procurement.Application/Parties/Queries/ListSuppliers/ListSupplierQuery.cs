using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.BuildingBlocks.Application.Requests;
using Invoria.BuildingBlocks.Domain.Dtos;
using Invoria.Procurement.Contracts.Dtos;

namespace Invoria.Procurement.Application.Parties.Queries.ListSuppliers;

public class ListSupplierQuery : PagingParams, IQuery<PagingDto<SupplierDto>>
{
    public string? Name { get; set; }
    public string? Code { get; set; }
}
