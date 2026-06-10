using Invoria.BuildingBlocks.Core.Modularity;
using Invoria.Ordering.Application.Orders.Sagas;
using Invoria.Ordering.Application.Orders.Sagas.Activities;
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
        services.AddTransient<IHandleMessages<RecordOrderAllocationSagaActivity>, RecordOrderAllocationSagaActivityHandler>();
        services.AddTransient<IHandleMessages<ReviseOrderSagaActivity>, ReviseOrderSagaActivityHandler>();
        services.AddTransient<IHandleMessages<MarkOrderAllocatedSagaActivity>, MarkOrderAllocatedSagaActivityHandler>();
        services.AddTransient<IHandleMessages<RecordOrderReturnSagaActivity>, RecordOrderReturnSagaActivityHandler>();
    }
}
