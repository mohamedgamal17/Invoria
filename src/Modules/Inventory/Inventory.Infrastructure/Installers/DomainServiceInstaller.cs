using Invoria.BuildingBlocks.Core.Modularity;
using Invoria.Inventory.Domain.Allocations.Services;
using Invoria.Inventory.Domain.Returns.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.Inventory.Infrastructure.Installers;

public sealed class DomainServiceInstaller : IServiceInstaller
{
    public void Install(IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IAllocationDomainService, AllocationDomainService>();
        services.AddTransient<IReturnDomainService, ReturnDomainService>();
    }
}
