using Invoria.Ordering.Application.Orders.Extensions;
using Invoria.Ordering.Contracts.Orders.Events;
using Rebus.Bus;
using Rebus.Handlers;
using Rebus.Sagas;

namespace Invoria.Ordering.Application.Orders.Sagas;

public sealed class OrderSaga : Saga<OrderSagaState>,
    IAmInitiatedBy<OrderCreatedIntegrationEvent>,
    IHandleMessages<OrderCreatedIntegrationEvent>,
    IHandleMessages<OrderAcceptedIntegrationEvent>
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
    }

    public Task Handle(OrderCreatedIntegrationEvent message)
    {
        if (!IsNew)
        {
            return Task.CompletedTask;
        }

        Data.ApplyCreated(message.Order);

        return Task.CompletedTask;
    }

    public async Task Handle(OrderAcceptedIntegrationEvent message)
    {
        if (IsNew || Data.State == OrderSagaProcessState.Allocating)
        {
            return;
        }

        Data.ApplyAccepted(message.Order);

        await _bus.Publish(message.Order.ToAllocateOrderIntegrationEvent());
    }
}
