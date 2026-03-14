using FastEndpoints;
using Invoria.BuildingBlocks.Infrastructure.Endpoints;
using Microsoft.Extensions.DependencyInjection;

namespace Invoria.BuildingBlocks.Infrastructure;

public static class FastEndpointsExtensions
{

    public static IServiceCollection AddModuleFastEndpoints(this IServiceCollection services)
    {
        services.AddFastEndpoints(options =>
        {
            options.Assemblies = EndpointsAssemblyRegistry.GetAssemblies();
        });

        return services;
    }
}
