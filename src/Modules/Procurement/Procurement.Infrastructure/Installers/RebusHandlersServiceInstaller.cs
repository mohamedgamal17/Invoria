using Invoria.BuildingBlocks.Core.Modularity;
using Invoria.Procurement.Application.ReportPurchaseOrdersCompletedMetrics.Consumers;
using Invoria.Procurement.Application.ReportPurchaseSalesMetrics.Consumers;
using Invoria.Procurement.Contracts.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Handlers;

namespace Invoria.Procurement.Infrastructure.Installers;

public sealed class RebusHandlersServiceInstaller : IServiceInstaller
{
    public void Install(IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IHandleMessages<PurchaseOrderCompletedIntegrationEvent>, RecordPurchaseOrdersCompletedMetricsIntegrationEventConsumer>();
        services.AddTransient<IHandleMessages<PurchaseOrderCompletedIntegrationEvent>, RecordPurchaseSalesMetricsIntegrationEventConsumer>();
    }
}