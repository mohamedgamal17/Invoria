namespace Invoria.BuildingBlocks.Domain.Entities;

public interface IAuditedEntity
{
    DateTimeOffset CreatedAt { get; }
    string? CreatedBy { get; }
    DateTimeOffset? LastModifiedAt { get; }
    string? LastModifiedBy { get; }
}

public interface IAuditedEntity<out TId> : IEntity<TId>, IAuditedEntity
{
}

