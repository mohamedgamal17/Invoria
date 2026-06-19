using Invoria.BuildingBlocks.Core.Modularity;
using Invoria.Ordering.Domain.Invoices.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Ordering.Infrastructure.Installers;

public sealed class DomainServiceInstaller : IServiceInstaller
{
    public void Install(IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IInvoiceDomainService, InvoiceDomainService>();
    }
}
