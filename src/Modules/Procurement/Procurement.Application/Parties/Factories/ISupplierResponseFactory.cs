using Invoria.BuildingBlocks.Application.Factories;
using Invoria.Procurement.Contracts.Dtos;
using Invoria.Procurement.Domain.Parties;

namespace Invoria.Procurement.Application.Parties.Factories;

public interface ISupplierResponseFactory : IResponseFactory<Supplier, SupplierDto>
{
}

