using Invoria.BuildingBlocks.Core.Modularity;
using Invoria.Ordering.Application.Invoices.Consumers;
using Invoria.Ordering.Application.Invoices.Sagas;
using Invoria.Ordering.Application.Invoices.Sagas.Activities;
using Invoria.Ordering.Application.Orders.Sagas;
using Invoria.Ordering.Application.Orders.Sagas.Activities;
using Invoria.Ordering.Contracts.Invoices.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Handlers;

namespace Invoria.Ordering.Infrastructure.Installers;

public sealed class RebusHandlersServiceInstaller : IServiceInstaller
{
    public void Install(IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<OrderSaga>();
        services.AddTransient<OrderReturnSaga>();
        services.AddTransient<OrderInvoiceSaga>();
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
