using System.Reflection;
using Invoria.BuildingBlocks.Domain.Entities;
using Invoria.BuildingBlocks.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Invoria.BuildingBlocks.EntityFramework.Hooks;

public sealed class DispatchEntityCrudDomainEventsAfterSaveHook : IAfterDbHookSave
{
    private static readonly MethodInfo PublishCreatedDefinition =
        typeof(DispatchEntityCrudDomainEventsAfterSaveHook).GetMethod(
            nameof(PublishCreatedImpl),
            BindingFlags.Static | BindingFlags.NonPublic)!;

    private static readonly MethodInfo PublishUpdatedDefinition =
        typeof(DispatchEntityCrudDomainEventsAfterSaveHook).GetMethod(
            nameof(PublishUpdatedImpl),
            BindingFlags.Static | BindingFlags.NonPublic)!;

    private static readonly MethodInfo PublishDeletedDefinition =
        typeof(DispatchEntityCrudDomainEventsAfterSaveHook).GetMethod(
            nameof(PublishDeletedImpl),
            BindingFlags.Static | BindingFlags.NonPublic)!;

    private readonly IPublisher _publisher;
    private readonly IEntityCrudChangeAccumulator _accumulator;

    public DispatchEntityCrudDomainEventsAfterSaveHook(
        IPublisher publisher,
        IEntityCrudChangeAccumulator accumulator)
    {
        _publisher = publisher;
        _accumulator = accumulator;
    }

    public async Task OnAfterSaveAsync(DbContext dbContext, CancellationToken cancellationToken = default)
    {
        foreach (var (entity, kind) in _accumulator.Drain())
        {
            var entityType = entity.GetType();
            var idType = ResolveIEntityIdType(entityType);
            if (idType is null)
            {
                continue;
            }

            var definition = kind switch
            {
                EntityCrudKind.Created => PublishCreatedDefinition,
                EntityCrudKind.Updated => PublishUpdatedDefinition,
                EntityCrudKind.Deleted => PublishDeletedDefinition,
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
            };

            var method = definition.MakeGenericMethod(entityType, idType);
            var invokeTask = (Task)method.Invoke(null, new object[] { _publisher, entity, cancellationToken })!;
            await invokeTask.ConfigureAwait(false);
        }
    }

    private static Type? ResolveIEntityIdType(Type entityType)
    {
        foreach (var iface in entityType.GetInterfaces())
        {
            if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IEntity<>))
            {
                return iface.GetGenericArguments()[0];
            }
        }

        return null;
    }

    private static Task PublishCreatedImpl<TEntity, TId>(
        IPublisher publisher,
        object entity,
        CancellationToken cancellationToken)
        where TEntity : class, IEntity<TId>
    {
        return publisher.Publish(new EntityCreatedDomainEvent<TEntity, TId>((TEntity)entity), cancellationToken);
    }

    private static Task PublishUpdatedImpl<TEntity, TId>(
        IPublisher publisher,
        object entity,
        CancellationToken cancellationToken)
        where TEntity : class, IEntity<TId>
    {
        return publisher.Publish(new EntityUpdatedDomainEvent<TEntity, TId>((TEntity)entity), cancellationToken);
    }

    private static Task PublishDeletedImpl<TEntity, TId>(
        IPublisher publisher,
        object entity,
        CancellationToken cancellationToken)
        where TEntity : class, IEntity<TId>
    {
        return publisher.Publish(new EntityDeletedDomainEvent<TEntity, TId>((TEntity)entity), cancellationToken);
    }
}
