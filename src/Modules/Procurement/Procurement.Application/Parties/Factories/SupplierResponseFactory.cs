using Invoria.BuildingBlocks.Application.Factories;
using Invoria.Procurement.Contracts.Dtos;
using Invoria.Procurement.Domain.Parties;

namespace Invoria.Procurement.Application.Parties.Factories;

public sealed class SupplierResponseFactory : ResponseFactory<Supplier, SupplierDto>, ISupplierResponseFactory
{
    public override Task<SupplierDto> PrepareDto(Supplier view)
    {
        var dto = new SupplierDto
        {
            Id = view.Id,
            SupplierCode = view.SupplierCode,
            Name = view.Name,
            ContactEmail = view.ContactEmail,
            Phone = view.Phone
        };

        MapAudited(view, dto);

        return Task.FromResult(dto);
    }
}

