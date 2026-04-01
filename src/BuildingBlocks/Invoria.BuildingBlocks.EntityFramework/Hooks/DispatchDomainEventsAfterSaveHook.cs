using Invoria.BuildingBlocks.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Invoria.BuildingBlocks.EntityFramework.Hooks;

public sealed class DispatchDomainEventsAfterSaveHook : IAfterDbHookSave
{
    private readonly IPublisher _publisher;

    public DispatchDomainEventsAfterSaveHook(IPublisher publisher)
    {
        _publisher = publisher;
    }

    public async Task OnAfterSaveAsync(DbContext dbContext, CancellationToken cancellationToken = default)
    {
        var aggregatesWithEvents = dbContext.ChangeTracker
            .Entries()
            .Where(e => e.State != EntityState.Detached)
            .Select(e => e.Entity)
            .OfType<IAggregateRoot>()
            .Where(a => a.DomainEvents.Count > 0)
            .ToList();

        foreach (var aggregate in aggregatesWithEvents)
        {
            foreach (var domainEvent in aggregate.DomainEvents.ToList())
            {
                if (domainEvent is INotification notification)
                {
                    await _publisher.Publish(notification, cancellationToken).ConfigureAwait(false);
                }
            }

            aggregate.ClearDomainEvents();
        }
    }
}
