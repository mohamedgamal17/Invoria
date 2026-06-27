using Invoria.BuildingBlocks.Core.Modularity;
using Invoria.Inventory.Contracts.Allocations.Events;
using Invoria.Inventory.Contracts.Returns.Events;
using Invoria.Ordering.Application.Invoices.Consumers;
using Invoria.Ordering.Application.Invoices.Sagas;
using Invoria.Ordering.Application.Invoices.Sagas.Activities;
using Invoria.Ordering.Application.Orders.Sagas;
using Invoria.Ordering.Application.Orders.Sagas.Activities;
using Invoria.Ordering.Contracts.Invoices.Events;
using Invoria.Ordering.Contracts.Orders.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Handlers;

namespace Invoria.Ordering.Infrastructure.Installers;

public sealed class RebusHandlersServiceInstaller : IServiceInstaller
{
    public void Install(IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IHandleMessages<OrderCreatedIntegrationEvent>, OrderSaga>();
        services.AddTransient<IHandleMessages<OrderAcceptedIntegrationEvent>, OrderSaga>();
        services.AddTransient<IHandleMessages<AllocationCreatedIntegrationEvent>, OrderSaga>();
        services.AddTransient<IHandleMessages<AllocationFailedIntegrationEvent>, OrderSaga>();
        services.AddTransient<IHandleMessages<AllocationSucceededIntegrationEvent>, OrderSaga>();
        services.AddTransient<IHandleMessages<OrderRevisionRequestedIntegrationEvent>, OrderSaga>();
        services.AddTransient<IHandleMessages<AllocationReleasedIntegrationEvent>, OrderSaga>();
        services.AddTransient<IHandleMessages<OrderCompletedIntegrationEvent>, OrderSaga>();
        services.AddTransient<IHandleMessages<OrderReturnRequestedIntegrationEvent>, OrderReturnSaga>();
        services.AddTransient<IHandleMessages<ImmediateReturnCreatedIntegrationEvent>, OrderReturnSaga>();
        services.AddTransient<IHandleMessages<OrderInvoiceRequestIntegrationEvent>, OrderInvoiceSaga>();
        services.AddTransient<IHandleMessages<OrderInvoiceCreatedIntegrationEvent>, OrderInvoiceSaga>();
        services.AddTransient<IHandleMessages<RecordOrderAllocationSagaActivity>, RecordOrderAllocationSagaActivityHandler>();
        services.AddTransient<IHandleMessages<ReviseOrderSagaActivity>, ReviseOrderSagaActivityHandler>();
        services.AddTransient<IHandleMessages<MarkOrderAllocatedSagaActivity>, MarkOrderAllocatedSagaActivityHandler>();
        services.AddTransient<IHandleMessages<CreateOrderReturnSagaActivity>, CreateOrderReturnSagaActivityHandler>();
        services.AddTransient<IHandleMessages<CreateOrderInvoiceSagaActivity>, CreateOrderInvoiceSagaActivityHandler>();
        services.AddTransient<IHandleMessages<RecordOrderReturnSagaActivity>, RecordOrderReturnSagaActivityHandler>();
        services.AddTransient<IHandleMessages<RecordOrderInvoiceSagaActivity>, RecordOrderInvoiceSagaActivityHandler>();
        services.AddTransient<IHandleMessages<CreateOrderInvoiceIntegrationEvent>, CreateOrderInvoiceIntegrationEventConsumer>();
    }
}
