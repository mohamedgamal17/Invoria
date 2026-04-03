using Invoria.Ordering.Contracts.Events;
using Rebus.Handlers;

namespace Invoria.Inventory.Infrastructure.Events;

public sealed class AllocateOrderIntegrationEventHandler : IHandleMessages<AllocateOrderIntegrationEvent>
{
    public Task Handle(AllocateOrderIntegrationEvent message)
    {
        return Task.CompletedTask;
    }
}
