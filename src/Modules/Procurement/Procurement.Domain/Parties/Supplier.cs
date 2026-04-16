using Invoria.BuildingBlocks.Domain.Entities;

namespace Invoria.Procurement.Domain.Parties;

public sealed class Supplier : AuditedAggregateRoot
{
    public string SupplierCode { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? ContactEmail { get; private set; }
    public string? Phone { get; private set; }

    private Supplier()
    {
    }

    public static Supplier Create(
        string id,
        string supplierCode,
        string name,
        string? contactEmail,
        string? phone,
        string? createdBy)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Id cannot be empty.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(supplierCode))
        {
            throw new ArgumentException("Supplier code cannot be empty.", nameof(supplierCode));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.", nameof(name));
        }

        return new Supplier
        {
            Id = id,
            SupplierCode = supplierCode,
            Name = name,
            ContactEmail = contactEmail,
            Phone = phone,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = createdBy
        };
    }
}
