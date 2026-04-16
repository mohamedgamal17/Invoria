using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.Procurement.Contracts.Dtos;

namespace Invoria.Procurement.Application.Parties.Queries.GetSupplierById;

public sealed class GetSupplierByIdQuery : IQuery<SupplierDto>
{
    public string Id { get; set; } = string.Empty;
}
