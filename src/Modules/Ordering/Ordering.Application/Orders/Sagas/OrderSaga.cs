using Invoria.Inventory.Contracts.Allocations.Events;
using Invoria.Ordering.Application.Orders.Extensions;
using Invoria.Ordering.Application.Orders.Sagas.Activities;
using Invoria.Ordering.Contracts.Orders.Events;
using Rebus.Bus;
using Rebus.Handlers;
using Rebus.Sagas;

namespace Invoria.Ordering.Application.Orders.Sagas;

public sealed class OrderSaga : Saga<OrderSagaState>,
    IAmInitiatedBy<OrderCreatedIntegrationEvent>,
    IHandleMessages<OrderCreatedIntegrationEvent>,
    IHandleMessages<OrderAcceptedIntegrationEvent>,
    IHandleMessages<AllocationCreatedIntegrationEvent>,
    IHandleMessages<AllocationFailedIntegrationEvent>,
    IHandleMessages<AllocationSucceededIntegrationEvent>
{
    private readonly IBus _bus;

    public OrderSaga(IBus bus)
    {
        _bus = bus;
    }

    protected override void CorrelateMessages(ICorrelationConfig<OrderSagaState> config)
    {
        config.Correlate<OrderCreatedIntegrationEvent>(m => m.Order.Id, d => d.OrderId);
        config.Correlate<OrderAcceptedIntegrationEvent>(m => m.Order.Id, d => d.OrderId);
        config.Correlate<AllocationCreatedIntegrationEvent>(m => m.Allocation.OrderId, d => d.OrderId);
        config.Correlate<AllocationFailedIntegrationEvent>(m => m.OrderId, d => d.OrderId);
        config.Correlate<AllocationSucceededIntegrationEvent>(m => m.OrderId, d => d.OrderId);
    }

    public Task Handle(OrderCreatedIntegrationEvent message)
    {
        Data.ApplyCreated(message.Order);

        return Task.CompletedTask;
    }

    public async Task Handle(OrderAcceptedIntegrationEvent message)
    {
        Data.ApplyAccepted(message.Order);

        await _bus.Publish(message.Order.ToAllocateOrderIntegrationEvent());
    }

    public async Task Handle(AllocationCreatedIntegrationEvent message)
    {
        Data.ApplyAllocationCreated(message.Allocation);

        await _bus.Publish(new RecordOrderAllocationSagaActivity(
            message.Allocation.OrderId,
            message.Allocation.Id));
    }

    public async Task Handle(AllocationFailedIntegrationEvent message)
    {
        Data.ApplyAllocationFailed(message.AllocationId);

        await _bus.Publish(new ReviseOrderSagaActivity(message.OrderId));
    }

    public async Task Handle(AllocationSucceededIntegrationEvent message)
    {
        Data.ApplyAllocationSucceeded(message.AllocationId);

        await _bus.Publish(new MarkOrderAllocatedSagaActivity(message.OrderId));
    }
}
