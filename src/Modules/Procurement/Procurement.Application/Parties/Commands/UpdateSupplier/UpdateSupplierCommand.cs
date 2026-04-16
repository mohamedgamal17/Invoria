using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.Procurement.Contracts.Dtos;

namespace Invoria.Procurement.Application.Parties.Commands.UpdateSupplier;

public sealed class UpdateSupplierCommand : ICommand<SupplierDto>
{
    public string Id { get; set; }
    public string SupplierCode { get; set; }
    public string Name { get; set; }
    public string? ContactEmail { get; set; }
    public string? Phone { get; set; }

    public UpdateSupplierCommand(string id, string supplierCode, string name, string? contactEmail, string? phone)
    {
        Id = id;
        SupplierCode = supplierCode;
        Name = name;
        ContactEmail = contactEmail;
        Phone = phone;
    }
}

