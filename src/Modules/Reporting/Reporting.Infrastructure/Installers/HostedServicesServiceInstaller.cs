using Invoria.BuildingBlocks.Core.Modularity;
using Invoria.Reporting.Application.Orders.Materialization.DebtSummary;
using Invoria.Reporting.Application.Orders.Materialization.OrderPeriodSummary;
using Invoria.Reporting.Application.Orders.Materialization.StatusSummary;
using Invoria.Reporting.Infrastructure.Orders.Materialization.DebtSummary;
using Invoria.Reporting.Infrastructure.Orders.Materialization.OrderPeriodSummary;
using Invoria.Reporting.Infrastructure.Orders.Materialization.StatusSummary;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Reporting.Infrastructure.Installers;

public sealed class HostedServicesServiceInstaller : IServiceInstaller
{
    public void Install(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IReportedOrderStatusSummaryRollupRefresher, ReportedOrderStatusSummaryRollupRefresher>();
        services.AddScoped<IOrderPeriodSummaryRollupRefresher, OrderPeriodSummaryRollupRefresher>();
        services.AddScoped<IDebtSummaryRollupRefresher, DebtSummaryRollupRefresher>();
        services.AddHostedService<ReportedOrderStatusSummaryRollupRefreshHostedService>();
    }
}
