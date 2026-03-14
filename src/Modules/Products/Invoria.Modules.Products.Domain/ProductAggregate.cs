using Invoria.BuildingBlocks.Domain.Entities;

namespace Invoria.Modules.Products.Domain;

public class ProductAggregate : AuditedAggregateRoot<Guid>
{
    private ProductAggregate()
    {
    }

    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }

    public static ProductAggregate Create(string name, string? description = null)
    {
        var product = new ProductAggregate
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            CreatedAt = DateTimeOffset.UtcNow
        };

        return product;
    }
}

