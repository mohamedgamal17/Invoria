namespace Invoria.BuildingBlocks.Domain.Entities;

public abstract class AuditedAggregateRoot<TId> : AggregateRoot<TId>, IAuditedEntity<TId>
{
    public DateTimeOffset CreatedAt { get; protected set; }
    public string? CreatedBy { get; protected set; }
    public DateTimeOffset? LastModifiedAt { get; protected set; }
    public string? LastModifiedBy { get; protected set; }
}


public class AuditedAggregateRoot : AuditedAggregateRoot<string> , IAggregateRoot
{
}

