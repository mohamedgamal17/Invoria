using Invoria.BuildingBlocks.Application.Abstractions.Cqrs;
using Invoria.Procurement.Contracts.Dtos;

namespace Invoria.Procurement.Application.Parties.Commands.CreateSupplier;

public sealed class CreateSupplierCommand : ICommand<SupplierDto>
{
    public string Name { get; set; }
    public string? ContactEmail { get; set; }
    public string? Phone { get; set; }

    public CreateSupplierCommand(string name, string? contactEmail, string? phone)
    {
        Name = name;
        ContactEmail = contactEmail;
        Phone = phone;
    }
}
